using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Permissions;
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
        private readonly ManualResetEvent _waitHandle;
        private bool _isRunning = true;
        private readonly NetworkPorts _ports;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly StateSaveConcurrentQueue<QueuedFile> _queue;
        private readonly RegistryKey _registry = Registry.CurrentUser.CreateSubKey(@"TorPdos\1.1.1.1");
        private readonly string _path;
        private bool _pendingReceiver = true;
        private FileSender _sender;
        private Receiver _receiver;
        private readonly Logger _logger = LogManager.GetLogger("UploadLogger");
        private bool _isStopped;
        private ConcurrentDictionary<string, List<string>> _sentTo;
        private readonly HashHandler _hashList;
        private readonly HiddenFolder _hiddenFolder;

        public ConcurrentDictionary<string, List<string>> SentTo{
            private get => _sentTo;
            set{
                if (_sentTo == null) _sentTo = value;
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public UploadManager(StateSaveConcurrentQueue<QueuedFile> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, HashHandler hashList){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;

            this._waitHandle = new ManualResetEvent(false);
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            _hiddenFolder = new HiddenFolder(_path + @".hidden");

            this._path = _registry.GetValue("Path").ToString();
            _hashList = hashList;
        }

        private void QueueElementAddedToQueue(){
            this._waitHandle.Set();
        }

        public void Run(){
            _isStopped = false;
            this._waitHandle.Set();

            while (_isRunning){
                this._waitHandle.WaitOne();

                if (!_isRunning)
                    break;

                var peersSentTo = new List<string>();

                while (this._queue.TryDequeue(out var file)){
                    if (!_isRunning){
                        this._queue.Enqueue(file);
                        break;
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
                    bool compressionCompleted = Compressor.CompressFile(filePath, compressedFilePath);
                    
                    if (!compressionCompleted){
                        this._queue.Enqueue(file);
                        continue;
                    }

                    // Encrypt file
                    var encryption = new FileEncryption(compressedFilePath, ".lzma");

                    bool encryptionCompleted = encryption.DoEncrypt(IdHandler.GetKeyMold());
                    

                    if (!encryptionCompleted){
                        this._queue.Enqueue(file);
                        continue;
                    }
                    
                    _hiddenFolder.Remove(compressedFilePath + ".lzma");

                    string encryptedFilePath = compressedFilePath + ".aes";

                    // Split
                    var splitter = new SplitterLibrary();
                    
                    _hashList.Add(file.GetHash(),
                        splitter.SplitFile(encryptedFilePath, file.GetHash(), _path + @".hidden\splitter\"));


                    foreach (var currentFileHashes in _hashList.GetEntry(file.GetHash())){
                        peersSentTo.Clear();
                        string currentFileHashPath = _path + @".hidden\splitter\" + currentFileHashes;

                        foreach (Peer peer in receivingPeers){
                            int port = _ports.GetAvailablePort();
                            
                            Console.WriteLine(port);
                            try{
                                _receiver = new Receiver(port);
                                _receiver.MessageReceived += this._receiver_MessageReceived;
                                _receiver.Start();
                            }
                            catch (SocketException e){
                                _logger.Log(LogLevel.Fatal, e);
                                Console.WriteLine(port);
                            }
                            catch (Exception e){
                                _logger.Warn(e);
                            }

                            var fileInfo = new FileInfo(currentFileHashPath);

                            var upload = new UploadMessage(peer){
                                filesize = fileInfo.Length,
                                filename = currentFileHashes,
                                filehash = file.GetHash(),
                                path = currentFileHashPath,
                                port = port
                            };
                            upload.Send();
                            Console.WriteLine(currentFileHashes);
                            int pendingCount = 0;
                            while (_pendingReceiver){
                                pendingCount++;
                                System.Threading.Thread.Sleep(1000);
                                if (pendingCount == 3){
                                    _receiver.Stop();
                                    _queue.Enqueue(file);
                                    break;
                                }
                            }


                            if (_sender != null){
                                _sender.Send(currentFileHashPath);
                                peersSentTo.Add(peer.UUID);
                            }

                            _pendingReceiver = true;
                            _ports.Release(port);
                        }


                        SentTo.AddOrUpdate(currentFileHashes, peersSentTo, (key, existingValue) => {
                            foreach (string peer in peersSentTo){
                                if (!existingValue.Contains(peer)){
                                    existingValue.Add(peer);
                                }
                            }

                            return existingValue;
                        });
                    }

                    foreach (string currentFileHash in _hashList.GetEntry(file.GetHash())){
                        File.Delete(_path + @".hidden\splitter\" + currentFileHash);
                    }
                }


                this._waitHandle.Reset();
            }

            _isStopped = true;
        }

        private void _receiver_MessageReceived(BaseMessage msg){
            if (msg.GetMessageType() == typeof(UploadMessage)){
                var upload = (UploadMessage) msg;

                if (upload.type.Equals(Messages.TypeCode.RESPONSE)){
                    if (upload.statusCode == StatusCode.ACCEPTED){
                        _sender = new FileSender(upload.from, upload.port);
                        _pendingReceiver = false;
                    }
                }
            }
        }

        private List<Peer> GetPeers(int count){
            var availablePeers = new List<Peer>();
            
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
            _isRunning = false;
            _waitHandle.Set();

            Console.Write("Upload thread stopping... ");
            while (!this._isStopped){ }

            Console.Write("Stopped!\n");

            return true;
        }
    }
}