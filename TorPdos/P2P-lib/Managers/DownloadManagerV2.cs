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
        private string _fileHash;
        private readonly string _path;
        private readonly NetworkPorts _ports;
        private readonly ManualResetEvent _waitHandle;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private StateSaveConcurrentQueue<FileDownloader> _queue;
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
        private FileDownloader _currentQueuedFile;

        private ConcurrentDictionary<string, List<string>> _sentTo;

        public ConcurrentDictionary<string, List<string>> SentTo{
            get => _sentTo;
            set{
                if (_sentTo == null) _sentTo = value;
            }
        }

        public DownloadManagerV2(StateSaveConcurrentQueue<FileDownloader> queue, NetworkPorts ports,
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

            _receiver = new Receiver(_port);
            _receiver.MessageReceived += _receiver_MessageReceived;
            Peer.PeerSwitchedOnline += PeerWentOnlineCheck;

            _receiver.Start();
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

                    _fileHash = file.GetHash();
                    _currentQueuedFile = file;

                    try{
                        _fileList = _hashList.GetEntry(_fileHash);
                        _downloadQueue = _hashList.GetEntry(_fileHash);
                    }
                    catch (KeyNotFoundException e){
                        _logger.Warn(
                            e +
                            " \n Requested file not found in hashfile, located in \\hidden\\hashList.json. \n Requested hash: " +
                            _fileHash);
                        Console.WriteLine("File not found in hashlist, see log");
                        break;
                    }

                    if (_downloadQueue == null){
                        return;
                    }

                    var updatedDownloadQueue = new List<string>();

                    if (Directory.Exists(_path + @".hidden\" + @"incoming\" + _fileHash)){
                        foreach (var currentFile in _downloadQueue){
                            if (!File.Exists(_path + @".hidden\" + @"incoming\" + _fileHash + @"\" + currentFile)){
                                updatedDownloadQueue.Add(currentFile);
                            }
                        }
                    } else{
                        updatedDownloadQueue = _downloadQueue;
                    }

                    if (updatedDownloadQueue.Count == 0){
                        RestoreOriginalFile(_fileHash, true);
                    }

                    foreach (string currentFileHash in updatedDownloadQueue){
                        List<Peer> onlinePeers = this.GetPeers();
                        //See if any online peers have the file
                        var sentToPeers = new List<string>();

                        _sentTo.TryGetValue(currentFileHash, out sentToPeers);
                        onlinePeers = OnlinePeersWithFile(onlinePeers, sentToPeers);

                        if (onlinePeers.Count == 0){
                            _queue.Enqueue(file);
                            break;
                        }

                        Console.WriteLine($"Looking for: {currentFileHash}");
                        if (!_sentTo.ContainsKey(currentFileHash)){
                            DiskHelper.ConsoleWrite("File not on network");
                            continue;
                        }

                        foreach (var onlinePeer in onlinePeers){
                            var downloadMessage = new DownloadMessage(onlinePeer){
                                port = _port,
                                fullFileName = file.GetHash(),
                                filehash = currentFileHash
                            };
                            downloadMessage.Send();
                        }
                    }
                }

                this._waitHandle.Reset();
            }

            isStopped = true;
        }

        private void _receiver_MessageReceived(BaseMessage msg){
            if (msg.GetMessageType() == typeof(DownloadMessage)){
                var download = (DownloadMessage) msg;

                if (download.type.Equals(Messages.TypeCode.RESPONSE)){
                    if (download.statusCode == StatusCode.ACCEPTED){
                        download.CreateReply();
                        download.type = Messages.TypeCode.REQUEST;
                        download.statusCode = StatusCode.ACCEPTED;
                        download.port = _ports.GetAvailablePort();
                        this._fileReceiver =
                            new FileReceiver(
                                Directory.CreateDirectory(_path + @".hidden\" + @"incoming\" + download.fullFileName +
                                                          @"\")
                                    .FullName,
                                download.filehash, download.port);

                        _sentCount++;

                        this._fileReceiver.FileSuccessfullyDownloaded += FileReceiverFileSuccessfullyDownloaded;
                        this._fileReceiver.Start();
                        DiskHelper.ConsoleWrite("FileReceiver opened");
                        download.Send();
                        _ports.Release(download.port);
                    } else if (download.statusCode == StatusCode.FILE_NOT_FOUND){
                        Console.WriteLine("File not found at peer.");
                    }
                }
            }
        }

        private void FileReceiverFileSuccessfullyDownloaded(string path){
            DiskHelper.ConsoleWrite("File downloaded");
            _count++;
            RestoreOriginalFile(path);
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

        private List<Peer> OnlinePeersWithFile(List<Peer> peerlist, List<string> hasFile){
            var result = new List<Peer>();
            foreach (Peer peer in peerlist){
                if (hasFile.Contains(peer.UUID)){
                    result.Add(peer);
                }
            }

            return result;
        }

        private void RestoreOriginalFile(string path, bool forceRestore = false){
            List<string> currentFileList = _hashList.GetEntry(Path.GetFileName(path));
            if (!forceRestore){
                foreach (var currentFile in currentFileList){
                    if (File.Exists(_path + @".hidden\incoming\" + _fileHash + @"\" + currentFile))
                        continue;
                    _queue.Enqueue(_currentQueuedFile);
                    return;
                }
            }

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