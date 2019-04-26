using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using Index_lib;
using Newtonsoft.Json;
using P2P_lib.Messages;
using Microsoft.Win32;
using System.Timers;
using P2P_lib.Managers;
using TypeCode = P2P_lib.Messages.TypeCode;

namespace P2P_lib{
    public class Network{
        private int _numOfThreads = 5;
        private int _port;
        private bool _running = false;
        private Index _index;
        private Receiver _receive;
        private FileReceiver _fileReceiver;
        private string _path;
        private HiddenFolder _hiddenPath;
        private BlockingCollection<Peer> peers = new BlockingCollection<Peer>();
        private string _peerFilePath;
        private RegistryKey registry = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
        private StateSaveConcurrentQueue<QueuedFile> upload;
        private StateSaveConcurrentQueue<QueuedFile> download;
        private List<Thread> threads = new List<Thread>();
        private NetworkPorts ports = new NetworkPorts();
        private System.Timers.Timer pingTimer;

        private static NLog.Logger _logger = NLog.LogManager.GetLogger("NetworkLogging");

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Network(int port, Index index, string path = "C:\\TorPdos\\"){
            this._port = port;
            this._path = path;
            this._index = index;
            this._peerFilePath = path + @".hidden\peer.json";
            _hiddenPath = new HiddenFolder(_path + @"\.hidden\");
            
            upload = new StateSaveConcurrentQueue<QueuedFile>(_path+@"hidden\uploadQueue.json");
            download = new StateSaveConcurrentQueue<QueuedFile>(_path + @"hidden\downloadQueue.json");
            Load();
        }

        public List<Peer> GetPeerList(){
            List<Peer> newPeerList = new List<Peer>();

            foreach (Peer peer in peers){
                newPeerList.Add(peer);
            }

            return newPeerList;
        }

        public void Start(){
            this._running = true;

            _receive = new Receiver(this._port);
            _receive.MessageReceived += Receive_MessageReceived;
            _receive.Start();

            for (int i = 0; i < _numOfThreads; i++){
                UploadManager uploadmanager = new UploadManager(upload, ports, peers);
                DownloadManager downloadmanager = new DownloadManager(download, ports, peers, _index);


                Thread uploadThread = new Thread(uploadmanager.Run);
                Thread downloadThread = new Thread(downloadmanager.Run);

                uploadThread.Start();
                downloadThread.Start();

                threads.Add(uploadThread);
                threads.Add(downloadThread);
            }

            pingTimer = new System.Timers.Timer();
            pingTimer.Interval = 10000;

            // Hook up the Elapsed event for the timer. 
            pingTimer.Elapsed += PingTimer_Elapsed;

            // Have the timer fire repeated events (true is the default)
            pingTimer.AutoReset = true;

            // Start the timer
            pingTimer.Enabled = true;
        }

        private void PingTimer_Elapsed(object sender, ElapsedEventArgs e){
            this.Ping();
        }

        public void Ping(){
            long millis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            foreach (Peer peer in peers){
                peer.Ping(millis);
            }
        }

        public void AddPeer(string uuid, string ip){
            Peer peer = new Peer(uuid, ip);
            this.peers.Add(peer);
        }

        private void Receive_MessageReceived(BaseMessage message){
            Type msgType = message.GetMessageType();

            if (msgType == typeof(PingMessage)){
                RechievedPing((PingMessage) message);
            } else if (msgType == typeof(UploadMessage)){
                ReceivedUpload((UploadMessage) message);
            } else if (msgType == typeof(DownloadMessage)){
                ReceivedDownloadMessage((DownloadMessage) message);
            } else if (msgType == typeof(PeerFetcherMessage)){
                RechievedPeerFetch((PeerFetcherMessage) message);
            }
        }

        private void RechievedPeerFetch(PeerFetcherMessage message){
            if (message.type.Equals(Messages.TypeCode.REQUEST)){
                List<Peer> incomming = new List<Peer>();
                List<Peer> outgoing = new List<Peer>();
                incomming = message.peers;
                // Adding sender to list
                if (!inPeerList(message.fromUuid, peers)){
                    peers.Add(new Peer(message.fromUuid, message.from));
                }

                //Checks whether a incomming peer exists in the peerlist.
                foreach (var incommingPeer in incomming){
                    if (inPeerList(incommingPeer.GetUuid(), peers)) break;
                    peers.Add(incommingPeer);
                    Console.WriteLine("Peer added: " + incommingPeer.GetUuid());
                }

                foreach (var outGoingPeer in peers){
                    if (inPeerList(outGoingPeer.GetUuid(), incomming)) break;
                    if (outGoingPeer.GetUuid() == message.fromUuid) break;
                    outgoing.Add(outGoingPeer);
                }

                message.CreateReply();
                message.peers = outgoing;
                message.Send();
            } else{
                // Rechieved response

                foreach (Peer incommingPeer in message.peers){
                    if (inPeerList(incommingPeer.GetUuid(), peers)) break;

                    if ((registry.GetValue("UUID").ToString().Equals(incommingPeer.GetUuid()))) break;
                    peers.Add(incommingPeer);
                    Console.WriteLine("Peer added: " + incommingPeer.GetUuid());
                }
            }

            // List peers in console. TODO this is for debugging purposes and should be removed
            //Console.WriteLine("My peers:");
        }

        private bool inPeerList(string uuid, List<Peer> input){
            bool inPeers = false;
            foreach (Peer peer in input){
                if (peer.GetUuid().Equals(uuid)){
                    inPeers = true;
                    break;
                }
            }

            // Add unknown peers to own list
            return inPeers;
        }

        public void SaveFile(){
            var json = JsonConvert.SerializeObject(peers);
            if (_path == null) return;
            using (var fileStream = _hiddenPath.WriteToFile(_peerFilePath)){
                var jsonIndex = new UTF8Encoding(true).GetBytes(json);
                fileStream.Write(jsonIndex, 0, jsonIndex.Length);
            }
        }

        public bool Load(){
            if (_peerFilePath == null || !File.Exists(this._peerFilePath)){
                string json = File.ReadAllText(this._peerFilePath ?? throw new NullReferenceException());
                List<Peer> input = JsonConvert.DeserializeObject<List<Peer>>(json);
                foreach (var peer in input){
                    peers.Add(peer);
                }

                return true;
            }

            return false;
        }

        private bool inPeerList(string uuid, BlockingCollection<Peer> input){
            bool inPeers = false;
            foreach (Peer peer in input){
                if (peer.GetUuid().Equals(uuid)){
                    inPeers = true;
                    break;
                }
            }

            // Add unknown peers to own list
            return inPeers;
        }

        private void ReceivedUpload(UploadMessage uploadMessage){
            if (uploadMessage.type.Equals(TypeCode.REQUEST)){
                int replyPort = uploadMessage.port;
                string uuid = uploadMessage.fromUuid;

                if (DiskHelper.getTotalFreeSpace("C:\\") > uploadMessage.filesize){
                    uploadMessage.statuscode = StatusCode.ACCEPTED;
                    Console.WriteLine(@"Request accepted");
                } else{
                    Console.WriteLine(@"Not enough space");
                    uploadMessage.statuscode = StatusCode.INSUFFICIENT_STORAGE;
                }

                uploadMessage.CreateReply();
                uploadMessage.port = ports.GetAvailablePort();

                _fileReceiver = new FileReceiver(this._path + @"\.hidden\" + uuid + @"\", uploadMessage.filename, uploadMessage.port,
                    true);
                _fileReceiver.Start();

                uploadMessage.Send(replyPort);
            }
        }

        private void RechievedPing(PingMessage ping){
            // Update peer
            foreach (Peer peer in peers){
                if (peer.GetUuid().Equals(ping.fromUuid)){
                    peer.SetIp(ping.from);
                    peer.UpdateLastSeen();
                    peer.SetOnline(true);
                }
            }

            // Respond to ping
            if (ping.type.Equals(TypeCode.REQUEST)){
                ping.CreateReply();
                ping.statuscode = StatusCode.OK;
                ping.Send();
            } else{
                // Recheved response, should send peerlist
                PeerFetcherMessage peerFetch = new PeerFetcherMessage(GetAPeer(ping.fromUuid));
                peerFetch.peers = this.GetPeerList();
                //Removed the rechiever from the list, as he should not add himself
                foreach (Peer peer in peerFetch.peers){
                    if (string.Compare(peer.GetIP().Trim(), peerFetch.to.Trim(), StringComparison.Ordinal) == 0){
                        peerFetch.peers.Remove(peer);
                        break;
                    }
                }

                peerFetch.Send();
            }
        }

        private Peer GetAPeer(string uuid){
            foreach (var peer in peers){
                if (peer.UUID.Equals(uuid)){
                    return peer;
                }
            }

            return null;
        }

        private void ReceivedDownloadMessage(DownloadMessage downloadMessage){
            if (downloadMessage.type.Equals(TypeCode.REQUEST)){
                if (downloadMessage.statuscode == StatusCode.OK){
                    Console.WriteLine(_path + @".hidden\" + downloadMessage.fromUuid + @"\" + downloadMessage.filehash + @".aes");
                    if (File.Exists(_path + @".hidden\" + downloadMessage.fromUuid + @"\" + downloadMessage.filehash + @".aes")){
                        downloadMessage.CreateReply();
                        downloadMessage.statuscode = StatusCode.ACCEPTED;
                        downloadMessage.Send(downloadMessage.port);
                        Console.WriteLine("Response send");
                    } else{
                        Console.WriteLine("File not found");
                        downloadMessage.CreateReply();
                        downloadMessage.statuscode = StatusCode.FILE_NOT_FOUND;
                        downloadMessage.Send(downloadMessage.port);
                        /*foreach (var peer in peers) {
                            download.forwardMessage(peer.GetIP());
                        }*/
                    }
                } else if (downloadMessage.statuscode == StatusCode.ACCEPTED){
                    var sender = new FileSender(downloadMessage.from, downloadMessage.port);
                    sender.Send(_path + @".hidden\" + downloadMessage.fromUuid + @"\" + downloadMessage.filehash + @".aes");
                    Console.WriteLine("File send");
                }
            }
        }

        public void Stop(){
            pingTimer.Enabled = false;

            foreach (Thread thread in threads){
                // TODO: Stop threads 
            }

            this._running = false;
            _receive.Stop();
        }

        public void UploadFileToNetwork(string filePath, int copies, int seed = 0){
            new NetworkProtocols(_index, this).UploadFileToNetwork(filePath, 3);
        }

        public void UploadFile(string hash, string path, int copies){
            this.upload.Enqueue(new QueuedFile(hash, path, copies));
        }

        public void DownloadFile(string hash){
            this.download.Enqueue(new QueuedFile(hash));
        }
    }
}