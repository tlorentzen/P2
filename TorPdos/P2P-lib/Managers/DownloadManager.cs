using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Compression;
using Encryption;
using Index_lib;
using P2P_lib.Handlers;
using P2P_lib.Helpers;
using Splitter_lib;

namespace P2P_lib.Managers{
    class DownloadManagerV2 : Manager{
        private bool _isRunning = true;
        private readonly int _port;
        private readonly string _path;
        private readonly NetworkPorts _ports;
        private readonly ManualResetEvent _waitHandle;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly StateSaveConcurrentQueue<P2PFile> _queue;
        private readonly Index _index;
        private List<string> _fileList;
        private bool isStopped;
        private readonly HashHandler _hashList;
        private List<string> _downloadQueue;
        private static NLog.Logger _logger = NLog.LogManager.GetLogger("DownloadLogger");
        private int _count = 0;
        private int _sentCount = 0;
        private ConcurrentQueue<FileDownloader> _queueBuilder;
        private string _fileHash;
        private FileDownloader _fileDownloader;

        public DownloadManagerV2(StateSaveConcurrentQueue<P2PFile> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, Index index){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
            this._path = DiskHelper.GetRegistryValue("Path");
            this._waitHandle = new ManualResetEvent(false);
            this._index = index;
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            this._port = _ports.GetAvailablePort();
            this._fileDownloader = new FileDownloader(ports, _peers);


            Peer.PeerSwitchedOnline += PeerWentOnlineCheck;
        }

        private void QueueElementAddedToQueue(){
            this._waitHandle.Set();
        }

        private void PeerWentOnlineCheck(){
            this._waitHandle.Set();
        }

        public void Run(){
            isStopped = false;
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

                    foreach (var path in _index.GetEntry(file.Hash).paths){
                        if (File.Exists(path)){
                            return;
                        }
                    }

                    _fileHash = file.Hash;
                    
                    foreach (var chunk in file.Chunks){
                        if (_fileDownloader.Fetch(chunk, file.Hash)) continue;
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

            isStopped = true;
        }

        private void RestoreOriginalFile(string path, P2PFile fileInformation){
            DiskHelper.ConsoleWrite("File exist");
            
            string pathWithoutExtension = (_path + @".hidden\incoming\" + _fileHash);

            //Merge files
            var splitterLibrary = new SplitterLibrary();
            splitterLibrary.MergeFiles(_path + @".hidden\incoming\" + _fileHash + @"\",
                pathWithoutExtension + ".aes",
                fileInformation.GetChunksAsString());


            // Decrypt file
            var decryption = new FileEncryption(pathWithoutExtension, ".lzma");
            decryption.DoDecrypt(IdHandler.GetKeyMold());
            DiskHelper.ConsoleWrite("File decrypted");
            File.Delete(path);

            // Decompress file
            string pathToFileForCopying =
                Compressor.DecompressFile(pathWithoutExtension + ".lzma", pathWithoutExtension);

            DiskHelper.ConsoleWrite("File decompressed");
            
            foreach (string filePath in _index.GetEntry(_fileHash).paths){
                if (!Directory.Exists(Path.GetDirectoryName(filePath))){
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                File.Copy(pathToFileForCopying, filePath);

                DiskHelper.ConsoleWrite($"File saved to: {filePath}");
            }
        }

        public override bool Shutdown(){
            _isRunning = false;
            _waitHandle.Set();

            Console.Write("Download thread stopping... ");
            while (!this.isStopped){ }

            Console.Write("Stopped!");
            return true;
        }
    }
}