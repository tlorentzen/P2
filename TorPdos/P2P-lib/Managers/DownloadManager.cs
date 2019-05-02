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
    public class DownloadManager : Manager{
        private bool is_running = true;
        private int _port;
        private string _filehash;
        private string originalFileHash;
        private string _path;
        private NetworkPorts _ports;
        private ManualResetEvent _waitHandle;
        private ConcurrentDictionary<string, Peer> _peers;
        private StateSaveConcurrentQueue<QueuedFile> _queue;
        private FileReceiver _receiver;
        private Index _index;
        private List<string> _filelist;
        public bool isStopped;
        private HashHandler _hashList;
        private List<string> _downloadQueue;
        private static NLog.Logger _logger = NLog.LogManager.GetLogger("DownloadLogger");

        private ConcurrentDictionary<string, List<string>> _sentTo;
        private List<Receiver> _receivers;

        public ConcurrentDictionary<string, List<string>> sentTo{
            get{ return _sentTo; }
            set{
                if (_sentTo == null) _sentTo = value;
            }
        }

        public DownloadManager(StateSaveConcurrentQueue<QueuedFile> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, Index index, HashHandler hashList){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
            this._path = DiskHelper.getRegistryValue("Path").ToString();
            this._waitHandle = new ManualResetEvent(false);
            this._index = index;
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            this._port = _ports.GetAvailablePort();
            _hashList = hashList;
        }

        private void QueueElementAddedToQueue(){
            this._waitHandle.Set();
        }

        public void Run(){
            isStopped = false;
            while (is_running){
                this._waitHandle.WaitOne();

                QueuedFile file;

                while (this._queue.TryDequeue(out file)){
                    if (!is_running){
                        this._queue.Enqueue(file);
                        break;
                    }

                    _filehash = file.GetHash();

                    try{
                        _filelist = _hashList.getEntry(_filehash);
                        _downloadQueue = _hashList.getEntry(_filehash);
                    }
                    catch (KeyNotFoundException e){
                        _logger.Warn(
                            e +
                            " \n Requested file not found in hashfile, located in \\hidden\\hashList.json. \n Requested hash: " +
                            _filehash);
                        Console.WriteLine("File not found in hashlist, see log");
                        break;
                    }


                    List<string> updatedDownloadQueue = new List<string>();
                    if (Directory.Exists(_path + @".hidden\" + @"incoming\" + _filehash)){
                        foreach (var currentFile in _downloadQueue){
                            if (!File.Exists(_path + @".hidden\" + @"incoming\" + _filehash + @"\" + currentFile)){
                                updatedDownloadQueue.Add(currentFile);
                            }
                        }
                    } else{
                        updatedDownloadQueue = _downloadQueue;
                    }


                    foreach (var currentFileHash in updatedDownloadQueue){
                        List<Peer> onlinePeers = this.GetPeers();
                        //See if any online peers have the file
                        List<string> sentToPeers = new List<string>();
                        _sentTo.TryGetValue(currentFileHash, out sentToPeers);
                        onlinePeers = OnlinePeersWithFile(onlinePeers, sentToPeers);


                        if (onlinePeers.Count == 0){
                            _queue.Enqueue(file);
                            break;
                        }


                        Console.WriteLine(currentFileHash);
                        if (!_sentTo.ContainsKey(currentFileHash)){
                            Console.WriteLine("File not on network");
                            continue;
                        }

                        int port;
                        port = _ports.GetAvailablePort();
                        Console.WriteLine("THIS IS A PORT: " + port);
                        Receiver receiver = new Receiver(port);
                        receiver.MessageReceived += _receiver_MessageReceived;
                        receiver.Start();
                        _receivers.Add(receiver);

                        foreach (var onlinePeer in onlinePeers){
                            DownloadMessage downloadMessage = new DownloadMessage(onlinePeer);
                            downloadMessage.port = port;
                            downloadMessage.fullFileName = file.GetHash();
                            downloadMessage.filehash = currentFileHash;
                            downloadMessage.Send();
                        }

                        Console.WriteLine("File: " + file.GetHash() + " was sent to?");
                    }
                }

                this._waitHandle.Reset();
            }

            isStopped = true;
        }

        private void _receiver_MessageReceived(BaseMessage msg){
            if (msg.GetMessageType() == typeof(DownloadMessage)){
                DownloadMessage download = (DownloadMessage) msg;
                if (download.type.Equals(Messages.TypeCode.RESPONSE)){
                    if (download.statuscode == StatusCode.ACCEPTED){
                        download.CreateReply();
                        download.type = Messages.TypeCode.REQUEST;
                        download.statuscode = StatusCode.ACCEPTED;
                        download.port = _ports.GetAvailablePort();
                        this._receiver =
                            new FileReceiver(
                                Directory.CreateDirectory(_path + @".hidden\" + @"incoming\" + download.fullFileName +
                                                          @"\")
                                    .FullName,
                                download.filehash, download.port, false);
                        this._receiver.FileSuccefullyDownloaded += _receiver_fileSuccefullyDownloaded;
                        this._receiver.Start();
                        Console.WriteLine("FileReceiver opened");
                        download.Send();
                        _ports.Release(download.port);
                    } else if (download.statuscode == StatusCode.FILE_NOT_FOUND){
                        Console.WriteLine("File not found at peer.");
                    }
                }
            }
        }

        private void _receiver_fileSuccefullyDownloaded(string path){
            Console.WriteLine("File downloaded");
            RestoreOriginalFile(path);
        }

        private List<Peer> GetPeers(){
            List<Peer> availablePeers = new List<Peer>();
            foreach (var peer in this._peers){
                if (peer.Value.IsOnline()){
                    availablePeers.Add(peer.Value);
                }
            }

            return availablePeers;
        }

        private List<Peer> OnlinePeersWithFile(List<Peer> peerlist, List<string> hasFile){
            List<Peer> result = new List<Peer>();
            foreach (Peer peer in peerlist){
                if (hasFile.Contains(peer.UUID)){
                    result.Add(peer);
                }
            }

            return result;
        }

        private void RestoreOriginalFile(string path){
            foreach (var currentFile in _filelist){
                if (!File.Exists(_path + @".hidden\incoming\" + _filehash + @"\" + currentFile)){
                    return;
                }

                try{
                    FileStream tester = new FileStream(_path + @".hidden\incoming\" + currentFile,
                        FileMode.Open,
                        FileAccess.Write);
                    tester.Close();
                }
                catch (Exception e){
                    return;
                }
            }

            Console.WriteLine("File exist");
            string pathWithoutExtension = (_path + @".hidden\incoming\" + _filehash);

            //Merge files
            SplitterLibary splitterLibrary = new SplitterLibary();
            splitterLibrary.MergeFiles(_path + @".hidden\incoming\" + _filehash + @"\",
                pathWithoutExtension + ".aes",
                _filelist);

            // Decrypt file
            FileEncryption decryption = new FileEncryption(pathWithoutExtension, ".lzma");
            decryption.DoDecrypt(IdHandler.GetKeyMold());
            Console.WriteLine("File decrypted");
            File.Delete(path);
            Console.WriteLine(pathWithoutExtension);

            // Decompress file
            string pathToFileForCopying =
                ByteCompressor.DecompressFile(pathWithoutExtension + ".lzma", pathWithoutExtension);
            Console.WriteLine("File decompressed");
            foreach (string filePath in _index.GetEntry(_filehash).paths){
                File.Copy(pathToFileForCopying, filePath);
                Console.WriteLine("File send to: {0}", filePath);
            }
        }

        public override bool Shutdown(){
            is_running = false;
            _waitHandle.Set();
            Console.Write("Download thread stopping... ");
            while (!this.isStopped){ }

            Console.Write("Stopped!");
            return true;
        }
    }
}