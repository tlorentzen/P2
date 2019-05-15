using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Compression;
using Encryption;
using Index_lib;
using P2P_lib.Messages;
using Splitter_lib;

namespace P2P_lib.Managers{
    class DownloadManagerV2 : Manager{
        private bool _isRunning = true;
        private readonly int _port;
        private readonly string _path;
        private readonly NetworkPorts _ports;
        private readonly ManualResetEvent _waitHandle;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private StateSaveConcurrentQueue<P2PFile> _queue;
        private FileReceiver _fileReceiver;
        private readonly Index _index;
        private List<string> _fileList;
        private bool isStopped;
        private readonly HashHandler _hashList;
        private List<string> _downloadQueue;
        private static NLog.Logger _logger = NLog.LogManager.GetLogger("DownloadLogger");
        private readonly Receiver _receiver;
        private int _count = 0;
        private int _sentCount = 0;
        private ConcurrentQueue<FileDownloader> _queueBuilder;
        private string _fileHash;
        private FileDownloader _fileDownloader;

        public DownloadManagerV2(StateSaveConcurrentQueue<P2PFile> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, Index index, HashHandler hashList){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
            this._path = DiskHelper.GetRegistryValue("Path");
            this._waitHandle = new ManualResetEvent(false);
            this._index = index;
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            this._port = _ports.GetAvailablePort();
            _hashList = hashList;
            

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

                while (this._queue.TryDequeue(out var file)){
                    if (!_isRunning){
                        this._queue.Enqueue(file);
                        break;
                    }

                    if (_queue == null){
                        return;
                    }

                    _fileHash = file.Hash;

                    foreach (var chunk in file.Chunks){
                        if (!_fileDownloader.Fetch(chunk)){
                            _queue.Enqueue(file);
                            break;
                        }
                    }



                    if (file.Downloaded(_path+@"\incoming")){
                        RestoreOriginalFile(_fileHash, true);
                    }
                    
                }

                this._waitHandle.Reset();
            }

            isStopped = true;
        }

        private List<Peer> GetPeers(){
            var availablePeers = new List<Peer>();

            foreach (var peer in this._peers){
                if (peer.Value.IsOnline()){
                    availablePeers.Add(peer.Value);
                }
            }

            return availablePeers;
        }

        private void RestoreOriginalFile(string path, bool forceRestore = false){
            DiskHelper.ConsoleWrite("File exist");
            string pathWithoutExtension = (_path + @".hidden\incoming\" + _fileHash);

            //Merge files
            var splitterLibrary = new SplitterLibrary();
            splitterLibrary.MergeFiles(_path + @".hidden\incoming\" + _fileHash + @"\",
                pathWithoutExtension + ".aes",
                _fileList);


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
            this._receiver.Stop();
            _waitHandle.Set();

            Console.Write("Download thread stopping... ");
            while (!this.isStopped){ }

            Console.Write("Stopped!");
            return true;
        }
    }
}