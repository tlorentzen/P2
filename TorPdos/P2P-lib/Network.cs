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
using P2P_lib;

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
        private ConcurrentDictionary<string, Peer> peers = new ConcurrentDictionary<string, Peer>();
        private string _peerFilePath;
        private RegistryKey registry = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
        private StateSaveConcurrentQueue<QueuedFile> upload;
        private StateSaveConcurrentQueue<QueuedFile> download;
        private StateSaveConcurrentQueue<string> _deletionQueue;
        private List<Manager> _managers = new List<Manager>();
        private NetworkPorts ports = new NetworkPorts();
        private System.Timers.Timer pingTimer;
        private string _locationDBPath;
        private ConcurrentDictionary<string, List<string>> locationDB;
        private DeletionManager _deletionManager;

        private static NLog.Logger _logger = NLog.LogManager.GetLogger("NetworkLogging");

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Network(int port, Index index, string path = "C:\\TorPdos\\"){
            this._port = port;
            this._path = path;
            this._index = index;
            this._peerFilePath = path + @".hidden\peer.json";
            this._locationDBPath = path + @".hidden\locationDB.json";
            _hiddenPath = new HiddenFolder(_path + @"\.hidden\");

            Load();

            _deletionQueue = StateSaveConcurrentQueue<string>.Load(_path + @".hidden\deletionQueue.json");
            upload = StateSaveConcurrentQueue<QueuedFile>.Load(_path + @".hidden\uploadQueue.json");
            download = StateSaveConcurrentQueue<QueuedFile>.Load(_path + @".hidden\downloadQueue.json");
            _deletionManager = new DeletionManager(_deletionQueue,ports,peers,locationDB);
        }

        public List<Peer> GetPeerList(){
            List<Peer> newPeerList = new List<Peer>();

            foreach(var peer in peers){
                newPeerList.Add(peer.Value);
            }
            
            return newPeerList;
        }

        public void Start(){
            this._running = true;

            _receive = new Receiver(this._port);
            _receive.MessageReceived += Receive_MessageReceived;
            _receive.Start();


            LoadLocationDB();

            for (int i = 0; i < _numOfThreads; i++){
                UploadManager uploadManager = new UploadManager(upload, ports, peers);
                DownloadManager downloadManager = new DownloadManager(download, ports, peers, _index);

                Thread uploadThread = new Thread(uploadManager.Run);
                Thread downloadThread = new Thread(downloadManager.Run);

                uploadManager.sentTo = downloadManager.sentTo = locationDB;

                uploadThread.Start();
                downloadThread.Start();

                _managers.Add(uploadManager);
                _managers.Add(downloadManager);
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

            foreach(var peer in peers){
                peer.Value.Ping(millis, _path);
            }
        }

        public void AddPeer(string uuid, string ip){
            Peer peer = new Peer(uuid, ip);
            this.peers.TryAdd(peer.GetUuid(), peer);
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
            } else if (msgType == typeof(FileDeletionMessage)){
                ReceivedDeletionRequest((FileDeletionMessage) message);
            }
        }

        private void RechievedPeerFetch(PeerFetcherMessage message){
            if (message.type.Equals(Messages.TypeCode.REQUEST)){
                List<Peer> incomming = new List<Peer>();
                List<Peer> outgoing = new List<Peer>();
                incomming = message.peers;
                // Adding sender to list
                if (!InPeerList(message.fromUuid, peers)){
                    peers.TryAdd(message.fromUuid, new Peer(message.fromUuid, message.from));
                }

                //Checks whether a incomming peer exists in the peerlist.
                foreach (var incommingPeer in incomming){
                    if (InPeerList(incommingPeer.GetUuid(), peers)) break;
                    peers.TryAdd(incommingPeer.GetUuid(), incommingPeer);
                    Console.WriteLine("Peer added: " + incommingPeer.GetUuid());
                }

                foreach (var outGoingPeer in peers){
                    if (InPeerList(outGoingPeer.Value.GetUuid(), incomming)) break;
                    if (outGoingPeer.Value.GetUuid() == message.fromUuid) break;
                    outgoing.Add(outGoingPeer.Value);
                }

                message.CreateReply();
                message.peers = outgoing;
                message.Send();
            } else{
                // Rechieved response

                foreach (Peer incommingPeer in message.peers){
                    if (InPeerList(incommingPeer.GetUuid(), peers)) break;

                    if ((IdHandler.GetUuid().Equals(incommingPeer.GetUuid()))) break;
                    peers.TryAdd(incommingPeer.GetUuid(), incommingPeer);
                    Console.WriteLine("Peer added: " + incommingPeer.GetUuid());
                }
            }

            // List peers in console. TODO this is for debugging purposes and should be removed
            //Console.WriteLine("My peers:");
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
            if (_peerFilePath != null && File.Exists(this._peerFilePath)){
                string json = File.ReadAllText(this._peerFilePath ?? throw new NullReferenceException());
                ConcurrentDictionary<string, Peer> input = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Peer>>(json);
                foreach (var peer in input){
                    peers.TryAdd(peer.Value.GetUuid(), peer.Value);
                }

                return true;
            }

            return false;
        }

        private bool InPeerList(string uuid, ConcurrentDictionary<string, Peer> input){
            bool inPeers = false;

            foreach(var peer in input){
                if (peer.Value.GetUuid().Equals(uuid))
                {
                    inPeers = true;
                    break;
                }
            }
           
            // Add unknown peers to own list
            return inPeers;
        }

        private bool InPeerList(string uuid, List<Peer> input){
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

                if (DiskHelper.getTotalAvailableSpace("C:\\") > uploadMessage.filesize){
                    uploadMessage.statuscode = StatusCode.ACCEPTED;
                    Console.WriteLine(@"Request accepted");
                } else{
                    Console.WriteLine(@"Not enough space");
                    uploadMessage.statuscode = StatusCode.INSUFFICIENT_STORAGE;
                }

                uploadMessage.CreateReply();
                uploadMessage.port = ports.GetAvailablePort();

                _fileReceiver = new FileReceiver(this._path + @"\.hidden\" + uuid + @"\", uploadMessage.filename,
                    uploadMessage.port,
                    true);
                _fileReceiver.Start();

                uploadMessage.Send(replyPort);
            }
        }

        private void RechievedPing(PingMessage ping){
            // Update peer
            foreach (var peer in peers){
                if (peer.Value.GetUuid().Equals(ping.fromUuid)){
                    peer.Value.SetIp(ping.from);
                    peer.Value.UpdateLastSeen();
                    peer.Value.SetOnline(true);
                    peer.Value.diskSpace = ping.diskSpace;
                }
            }

            // Respond to ping
            if (ping.type.Equals(TypeCode.REQUEST)){
                ping.CreateReply();
                ping.statuscode = StatusCode.OK;
                ping.diskSpace = DiskHelper.getTotalAvailableSpace(_path);
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
                if (peer.Value.UUID.Equals(uuid)){
                    return peer.Value;
                }
            }

            return null;
        }

        private void ReceivedDownloadMessage(DownloadMessage downloadMessage){
            if (downloadMessage.type.Equals(TypeCode.REQUEST)){
                if (downloadMessage.statuscode == StatusCode.OK){
                    Console.WriteLine(_path + @".hidden\" + downloadMessage.fromUuid + @"\" + downloadMessage.filehash +
                                      @".aes");
                    if (File.Exists(_path + @".hidden\" + downloadMessage.fromUuid + @"\" + downloadMessage.filehash +
                                    @".aes")){
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
                    sender.Send(_path + @".hidden\" + downloadMessage.fromUuid + @"\" + downloadMessage.filehash +
                                @".aes");
                    Console.WriteLine("File send");
                }
            }
        }

        private void ReceivedDeletionRequest(FileDeletionMessage message){
            if (message.type.Equals(TypeCode.REQUEST)){
                if (message.statuscode == StatusCode.OK){
                    if (File.Exists(_path + @".hidden\" + message.fromUuid + "\\" + message.filehash)){
                        File.Delete(_path + @".hidden\" + message.fromUuid + "\\" + message.filehash);
                        message.statuscode = StatusCode.OK;
                        message.CreateReply();
                        message.Send();
                    } else{
                        message.statuscode = StatusCode.FILE_NOT_FOUND;
                        message.CreateReply();
                        message.Send();
                    }
                }
            } else if (message.type == TypeCode.RESPONSE){
                if (message.statuscode == StatusCode.ACCEPTED){
                    List<string> updatedList = locationDB[message.filehash];
                    updatedList.Remove(message.filehash);
                    locationDB[message.filehash] = updatedList;
                } else{
                    Console.WriteLine("File not found at peer");
                }
            }
        }

        public void Stop(){
            pingTimer.Enabled = false;
            foreach (var manager in _managers){
                manager.Shutdown();
            }

            upload.Save(_path + @".hidden\uploadQueue.json");
            download.Save(_path + @".hidden\downloadQueue.json");
            _deletionQueue.Save(_path + @".hidden\deletionQueue.json");

            this._running = false;
            _receive.Stop();
            SaveLocationDB();
        }

        private void LoadLocationDB(){
            if (File.Exists(_locationDBPath)){
                locationDB =
                    JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<string>>>(
                        File.ReadAllText(_locationDBPath));
            } else{
                locationDB = new ConcurrentDictionary<string, List<string>>();
            }
        }

        private void SaveLocationDB(){
            var json = JsonConvert.SerializeObject(locationDB);
            if (_path == null) return;
            using (var fileStream = _hiddenPath.WriteToFile(_locationDBPath)){
                var jsonIndex = new UTF8Encoding(true).GetBytes(json);
                fileStream.Write(jsonIndex, 0, jsonIndex.Length);
            }
        }

        public void UploadFile(string hash, string path, int copies){
            this.upload.Enqueue(new QueuedFile(hash, path, copies));
        }

        public void DownloadFile(string hash){
            this.download.Enqueue(new QueuedFile(hash));
        }

        public void DeleteFile(string hash)
        {
            this._deletionQueue.Enqueue(hash);
        }
    }
}