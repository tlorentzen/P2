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
using NLog;
using P2P_lib.Messages;
using Splitter_lib;
using System.Linq;
using P2P_lib.Handlers;
using P2P_lib.Helpers;

namespace P2P_lib.Managers{
    public class UploadManager : Manager{
        private readonly ManualResetEvent _waitHandle;
        private bool _isRunning = true;
        private readonly NetworkPorts _ports;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly StateSaveConcurrentQueue<P2PFile> _queue;
        private readonly string _path;
        private bool _pendingReceiver = true;
        private FileSender _sender;
        private Receiver _receiver;
        private readonly Logger _logger = LogManager.GetLogger("UploadLogger");
        private bool _isStopped;
        private ConcurrentDictionary<string, List<string>> _sentTo;
        private readonly HiddenFolder _hiddenFolder;
        private int _numberOfPrimaryPeers = 10;


        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public UploadManager(StateSaveConcurrentQueue<P2PFile> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;

            this._waitHandle = new ManualResetEvent(false);
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            _hiddenFolder = new HiddenFolder(_path + @".hidden");

            this._path = DiskHelper.GetRegistryValue("Path");
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
                

                while (this._queue.TryDequeue(out P2PFile file)){
                    if (!_isRunning){
                        this._queue.Enqueue(file);
                        break;
                    }

                    int copies = 5;
                    string filePath = file.Paths[0];
                    string compressedFilePath = this._path + @".hidden\" + file.Hash;

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

                    // Initialize splitter
                    var splitter = new SplitterLibrary();

                    List<string> chunks =
                        splitter.SplitFile(encryptedFilePath, file.Hash, _path + @".hidden\splitter\");
                    while (!file.AddChunk(chunks)){
                        //Wait for all chunks to be added to list.
                    }

                    foreach (var chunk in file.Chunks){
                        string currentFileHashPath = _path + @".hidden\splitter\" + chunk.Hash;

                        foreach (Peer peer in receivingPeers){
                            int port = _ports.GetAvailablePort();
                            try{
                                _receiver = new Receiver(port);
                                _receiver.MessageReceived += this._receiver_MessageReceived;
                                _receiver.Start();
                            }
                            catch (SocketException e){
                                _logger.Log(LogLevel.Fatal, e);
                                DiskHelper.ConsoleWrite("Current port" + port);
                            }
                            catch (Exception e){
                                _logger.Warn(e);
                            }

                            var fileInfo = new FileInfo(currentFileHashPath);

                            var upload = new UploadMessage(peer){
                                filesize = fileInfo.Length,
                                filename = chunk.Hash,
                                filehash = file.Hash,
                                path = currentFileHashPath,
                                port = port
                            };

                            upload.Send();
                            DiskHelper.ConsoleWrite("Sending: " + chunk.Hash);
                            int pendingCount = 0;
                            while (_pendingReceiver){
                                pendingCount++;
                                Thread.Sleep(1000);
                                if (pendingCount == 3){
                                    _receiver.Stop();
                                    _queue.Enqueue(file);
                                    break;
                                }
                            }

                            if (_sender != null){
                                _sender.Send(currentFileHashPath);
                                chunk.AddPeer(peer.UUID);
                                
                                
                            }
                            _pendingReceiver = true;
                            _ports.Release(port);
                        }
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
            List<Peer> topPeers = _peers.Values.Where(peer => peer.IsOnline()).ToList();
            topPeers.Sort(new ComparePeersByRating());
            if (topPeers.Count > 0){
                int wantedLengthOfTopList = Math.Min(_numberOfPrimaryPeers, Math.Min(topPeers.Count, count));
                topPeers.RemoveRange(wantedLengthOfTopList, Math.Max(0, topPeers.Count - wantedLengthOfTopList));
            }

            return topPeers;
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