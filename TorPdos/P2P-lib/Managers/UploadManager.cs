using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Compression;
using Encryption;
using Index_lib;
using Microsoft.Win32;
using NLog;
using P2P_lib.Messages;
using Splitter_lib;

namespace P2P_lib.Managers{
    public class UploadManager : Manager{
        private ManualResetEvent waitHandle;
        private bool is_running = true;
        private NetworkPorts _ports;
        private ConcurrentDictionary<string, Peer> _peers;
        private StateSaveConcurrentQueue<QueuedFile> _queue;
        private RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"TorPdos\1.1.1.1");
        private string _path;
        private bool _pendingReceiver = true;
        private FileSender _sender;
        private Receiver _receiver;
        private static Logger logger = LogManager.GetLogger("UploadLogger");
        private HiddenFolder _hiddenFolder;
        public bool isStopped;
        private ConcurrentDictionary<string, List<string>> _sentTo;
        private HashHandler _hashList;

        public ConcurrentDictionary<string, List<string>> sentTo{
            get{ return _sentTo; }
            set{
                if (_sentTo == null) _sentTo = value;
            }
        }

        public UploadManager(StateSaveConcurrentQueue<QueuedFile> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, HashHandler hashList){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;

            this.waitHandle = new ManualResetEvent(false);
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;

            this._path = registry.GetValue("Path").ToString();
            _hiddenFolder = new HiddenFolder(this._path + @"\.hidden\");
            _hashList = hashList;
        }

        private void QueueElementAddedToQueue(){
            this.waitHandle.Set();
        }

        public void Run(){
            var outputFiles = new Dictionary<string, List<string>>();
            isStopped = false;
            this.waitHandle.Set();

            while (is_running){
                this.waitHandle.WaitOne();

                if (!is_running)
                    break;

                QueuedFile file;
                var peersSentTo = new List<string>();

                while (this._queue.TryDequeue(out file)){
                    if (!is_running){
                        this._queue.Enqueue(file);
                        break;
                    }

                    if (!Directory.Exists(_path + @"\.hidden\splitterOut\")){
                        Directory.CreateDirectory(_path + @"\.hidden\splitterOut\");
                    }

                    int copies = file.GetCopies();
                    string filePath = file.GetPath();
                    string compressedFilePath = this._path + @".hidden\" + file.GetHash();

                    List<Peer> receivingPeers = this.GetPeers(Math.Min(copies, this.CountOnlinePeers()));

                    if (receivingPeers.Count == 0){
                        this._queue.Enqueue(file);
                        continue;
                    }

                    // Compress file
                    ByteCompressor.CompressFile(filePath, compressedFilePath);

                    // Encrypt file
                    FileEncryption encryption = new FileEncryption(compressedFilePath, ".lzma");
                    encryption.DoEncrypt("password");
                    //_hiddenFolder.removeFile(compressedFilePath + ".lzma");
                    string encryptedFilePath = compressedFilePath + ".aes";

                    string filename = file.GetHash() + ".aes";

                    // Split
                    SplitterLibary splitter = new SplitterLibary();


                    _hashList.Add(file.GetHash(),
                        splitter.SplitFile(encryptedFilePath, file.GetHash(), _path + @"\.hidden\splitter\"));

                    
                    foreach (var currentFileHashes in _hashList.getEntry(file.GetHash())){
                        peersSentTo.Clear();
                        string currentFileHashPath = _path + @"\.hidden\splitter\" + currentFileHashes;
                        foreach (Peer peer in receivingPeers){
                            int port = _ports.GetAvailablePort();
                            
                            try{
                                _receiver = new Receiver(port);
                                _receiver.MessageReceived += this._receiver_MessageReceived;
                                _receiver.Start();
                            }
                            catch (SocketException e){
                                logger.Log(LogLevel.Fatal, e);
                            }
                            catch (Exception e){
                                logger.Warn(e);
                            }

                            FileInfo fileInfo = new FileInfo(currentFileHashPath);

                            UploadMessage upload = new UploadMessage(peer);
                            upload.filesize = fileInfo.Length;
                            upload.filename = currentFileHashes;
                            upload.filehash = currentFileHashes;
                            upload.path = currentFileHashPath;
                            upload.port = port;
                            upload.Send();
                            Console.WriteLine(currentFileHashes);

                            while (_pendingReceiver){
                                // TODO: timeout???
                            }

                            _receiver.Stop();
                            //_ports.Release(port);

                            if (_sender != null){
                                _sender.Send(currentFileHashPath);
                                peersSentTo.Add(peer.UUID);
                            }

                            _pendingReceiver = true;
                            _ports.Release(port);
                        }


                        sentTo.AddOrUpdate(currentFileHashes, peersSentTo, (key, existingValue) => {
                            foreach (string peer in peersSentTo){
                                if (!existingValue.Contains(peer)){
                                    existingValue.Add(peer);
                                }
                            }

                            return existingValue;
                        });
                    }
                }

                foreach (KeyValuePair<string, List<string>> currentFile in outputFiles){
                    foreach (var currentFileHashes in currentFile.Value){
                        File.Delete(_path + @"\.hidden\splitter\" + currentFileHashes);
                    }
                }

                this.waitHandle.Reset();
            }

            isStopped = true;
        }

        private void _receiver_MessageReceived(BaseMessage msg){
            if (msg.GetMessageType() == typeof(UploadMessage)){
                UploadMessage upload = (UploadMessage) msg;

                if (upload.type.Equals(Messages.TypeCode.RESPONSE)){
                    if (upload.statuscode == StatusCode.ACCEPTED){
                        _sender = new FileSender(upload.from, upload.port);
                        _pendingReceiver = false;
                    }
                }
            }
        }

        private List<Peer> GetPeers(int count){
            List<Peer> availablePeers = new List<Peer>();
            int counter = 1;

            foreach (var peer in this._peers){
                if (peer.Value.IsOnline()){
                    availablePeers.Add(peer.Value);

                    if (counter.Equals(count)){
                        break;
                    }

                    counter++;
                }
            }

            return availablePeers;
        }

        private int CountOnlinePeers(){
            int counter = 0;

            foreach (var peer in this._peers){
                if (peer.Value.IsOnline()){
                    counter++;
                }
            }

            return counter;
        }

        public override bool Shutdown(){
            is_running = false;
            waitHandle.Set();

            Console.Write("Upload thread stopping... ");
            while (!this.isStopped){ }

            Console.Write("Stopped!\n");

            return true;
        }
    }
}