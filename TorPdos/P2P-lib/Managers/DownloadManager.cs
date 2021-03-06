﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using Compression;
using Encryption;
using Index_lib;
using P2P_lib.Handlers;
using P2P_lib.Handlers.FileHandlers;
using P2P_lib.Helpers;
using Splitter_lib;

namespace P2P_lib.Managers{
    class DownloadManager : Manager{
        private bool _isRunning = true;
        private readonly string _path;
        private readonly NetworkPorts _ports;
        private readonly ManualResetEvent _waitHandle;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly StateSaveConcurrentQueue<P2PFile> _queue;
        private readonly Index _index;
        private bool _isStopped;
        private string _fileHash;
        private FileDownloader _fileDownloader;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("DownloadLoger");

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public DownloadManager(StateSaveConcurrentQueue<P2PFile> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, Index index){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
            this._path = DiskHelper.GetRegistryValue("Path");
            this._waitHandle = new ManualResetEvent(false);
            this._index = index;
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            _ports.GetAvailablePort();
            this._fileDownloader = new FileDownloader(ports, _peers);


            Peer.PeerSwitchedOnline += PeerWentOnlineCheck;
        }

        private void QueueElementAddedToQueue(){
            this._waitHandle.Set();
        }

        private void PeerWentOnlineCheck(){
            this._waitHandle.Set();
        }

        /// <summary>
        /// This is the function that needs to be run, for the DownloadManager to watch the queue.
        /// </summary>
        public void Run(){
            _isStopped = false;
            while (_isRunning){
                this._waitHandle.WaitOne();

                while (this._queue.TryDequeue(out P2PFile file)){
                    if (!_isRunning){
                        this._queue.Enqueue(file);
                        break;
                    }

                    if (_queue == null){
                        return;
                    }

                    foreach (string path in _index.GetEntry(file.hash).paths){
                        if (File.Exists(path)){
                            return;
                        }
                    }

                    _fileHash = file.hash;

                    foreach (var chunk in file.chunks){
                        if (_fileDownloader.Fetch(chunk, file.hash)){
                            continue;
                        }

                        this._queue.Enqueue(file);
                        break;
                    }

                    //Console.WriteLine(fileInformation.Downloaded(_path + @".hidden\incoming\"));
                    if (file.Downloaded(_path + @".hidden\incoming\")){
                        RestoreOriginalFile(_fileHash, file);
                    }
                }

                this._waitHandle.Reset();
            }

            _isStopped = true;
        }

        /// <summary>
        /// Restores the original file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileInformation"></param>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void RestoreOriginalFile(string path, P2PFile fileInformation){
            DiskHelper.ConsoleWrite("File exist");

            string pathWithoutExtension = (_path + @".hidden\incoming\" + fileInformation.hash);

            //Merge files
            var splitterLibrary = new SplitterLibrary();


            if (!splitterLibrary.MergeFiles(_path + @".hidden\incoming\" + fileInformation.hash + @"\",
                pathWithoutExtension + ".aes",
                fileInformation.GetChunksAsString())){
                _queue.Enqueue(fileInformation);
                return;
            }

            // Decrypt file
            var decryption = new FileEncryption(pathWithoutExtension, ".lzma");
            if (!decryption.DoDecrypt(IdHandler.GetKeyMold())){
                _queue.Enqueue(fileInformation);
                return;
            }

            DiskHelper.ConsoleWrite("File decrypted");

            File.Delete(path);

            // Decompress file
            string pathToFileForCopying =
                Compressor.DecompressFile(pathWithoutExtension + ".lzma", pathWithoutExtension);


            DiskHelper.ConsoleWrite("File decompressed");

            foreach (string filePath in _index.GetEntry(_fileHash).paths){
                if (!Directory.Exists(Path.GetDirectoryName(filePath))){
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new NullReferenceException());
                }

                try{
                    if (!File.Exists(filePath)){
                        File.Copy(pathToFileForCopying, filePath);
                        DiskHelper.ConsoleWrite($"File saved to: {filePath}");
                    }
                }
                catch (Exception e){
                    logger.Error(e);
                }
            }
        }

        /// <summary>
        /// Function shutdown.
        /// </summary>
        /// <returns>Returns true when shutdown successful</returns>
        public override bool Shutdown(){
            _isRunning = false;
            _waitHandle.Set();

            Console.Write("Download thread stopping... ");
            while (!this._isStopped){ }

            Console.Write("Stopped!");
            return true;
        }
    }
}