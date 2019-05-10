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
using System.Timers;
using P2P_lib.Managers;
using TypeCode = P2P_lib.Messages.TypeCode;
using Splitter_lib;
using System.Linq;

namespace P2P_lib{
    public class Network{
        private const int NumOfThreads = 5;
        private readonly int _port;
        private bool _running;
        private readonly Index _index;
        private Receiver _receive;
        private FileReceiver _fileReceiver;
        private readonly string _path;
        private readonly HiddenFolder _hiddenPath;
        private readonly ConcurrentDictionary<string, Peer> _peers = new ConcurrentDictionary<string, Peer>();
        private readonly string _peerFilePath;
        private readonly StateSaveConcurrentQueue<QueuedFile> _upload;
        private readonly StateSaveConcurrentQueue<QueuedFile> _download;
        private readonly StateSaveConcurrentQueue<string> _deletionQueue;
        private readonly List<Manager> _managers = new List<Manager>();
        private readonly NetworkPorts _ports = new NetworkPorts();
        private System.Timers.Timer _pingTimer;
        private readonly string _locationDbPath;
        private ConcurrentDictionary<string, List<string>> _locationDb;
        private DeletionManager _deletionManager;
        private readonly HashHandler _hashList;
        public List<string> topPeers;
        private int _numberOfPrimaryPeers = 10;


        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Network(int port, Index index, string path = "C:\\TorPdos\\"){
            this._port = port;
            this._path = path;
            this._index = index;
            this._peerFilePath = path + @".hidden\peer.json";
            this._locationDbPath = path + @".hidden\locationDB.json";
            _hiddenPath = new HiddenFolder(_path + @"\.hidden\");
            _hashList = new HashHandler(_path);

            Load();

            _deletionQueue = StateSaveConcurrentQueue<string>.Load(_path + @".hidden\deletionQueue.json");
            _upload = StateSaveConcurrentQueue<QueuedFile>.Load(_path + @".hidden\uploadQueue.json");
            _download = StateSaveConcurrentQueue<QueuedFile>.Load(_path + @".hidden\downloadQueue.json");
        }

        public List<Peer> GetPeerList(){
            List<Peer> newPeerList = new List<Peer>();

            foreach (var peer in _peers){
                newPeerList.Add(peer.Value);
            }

            return newPeerList;
        }

        public void Start(){
            this._running = true;

            _receive = new Receiver(this._port);
            _receive.MessageReceived += Receive_MessageReceived;
            _receive.Start();


            LoadLocationDb();
            _deletionManager = new DeletionManager(_deletionQueue, _ports, _peers, _locationDb, _hashList);
            Thread deletionManager = new Thread(_deletionManager.Run);
            deletionManager.Start();
            _managers.Add(_deletionManager);

            for (int i = 0; i < NumOfThreads; i++){
                UploadManager uploadManager = new UploadManager(_upload, _ports, _peers, _hashList);
                DownloadManagerV2 downloadManager = new DownloadManagerV2(_download, _ports, _peers, _index, _hashList);

                Thread uploadThread = new Thread(uploadManager.Run);
                Thread downloadThread = new Thread(downloadManager.Run);

                uploadManager.SentTo = downloadManager.SentTo = _locationDb;

                uploadThread.Start();
                downloadThread.Start();

                _managers.Add(uploadManager);
                _managers.Add(downloadManager);
            }

            _pingTimer = new System.Timers.Timer();
            _pingTimer.Interval = 10000;

            // Hook up the Elapsed event for the timer. 
            _pingTimer.Elapsed += PingTimer_Elapsed;

            // Have the timer fire repeated events (true is the default)
            _pingTimer.AutoReset = true;

            // Start the timer
            _pingTimer.Enabled = true;
        }

        private void PingTimer_Elapsed(object sender, ElapsedEventArgs e){
            this.Ping();
        }

        public void Ping(){
            long millis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            foreach (var peer in _peers){
                peer.Value.Ping(millis, _path);
            }
        }

        public void AddPeer(string uuid, string ip){
            Peer peer = new Peer(uuid, ip);
            this._peers.TryAdd(peer.GetUuid(), peer);
        }

        private void Receive_MessageReceived(BaseMessage message){
            Type msgType = message.GetMessageType();

            if (msgType == typeof(PingMessage)){
                ReceivedPing((PingMessage) message);
            } else if (msgType == typeof(UploadMessage)){
                ReceivedUpload((UploadMessage) message);
            } else if (msgType == typeof(DownloadMessage)){
                ReceivedDownloadMessage((DownloadMessage) message);
            } else if (msgType == typeof(PeerFetcherMessage)){
                ReceivedPeerFetch((PeerFetcherMessage) message);
            } else if (msgType == typeof(FileDeletionMessage)){
                ReceivedDeletionRequest((FileDeletionMessage) message);
            }
        }

        private void ReceivedPeerFetch(PeerFetcherMessage message){
            if (message.type.Equals(TypeCode.REQUEST)){
                List<Peer> outgoing = new List<Peer>();
                var incoming = message.peers;
                // Adding sender to list
                if (!InPeerList(message.fromUuid, _peers)){
                    _peers.TryAdd(message.fromUuid, new Peer(message.fromUuid, message.from));
                }

                //Checks whether a incomming peer exists in the peerlist.
                foreach (var incomingPeer in incoming){
                    if (InPeerList(incomingPeer.GetUuid(), _peers)) break;
                    _peers.TryAdd(incomingPeer.GetUuid(), incomingPeer);
                    Console.WriteLine("Peer added: " + incomingPeer.GetUuid());
                }

                foreach (var outGoingPeer in _peers){
                    if (InPeerList(outGoingPeer.Value.GetUuid(), incoming)) break;
                    if (outGoingPeer.Value.GetUuid() == message.fromUuid) break;
                    outgoing.Add(outGoingPeer.Value);
                }

                message.CreateReply();
                message.peers = outgoing;
                message.Send();
            } else{
                // Rechieved response

                foreach (Peer incommingPeer in message.peers){
                    if (InPeerList(incommingPeer.GetUuid(), _peers)) break;

                    if ((IdHandler.GetUuid().Equals(incommingPeer.GetUuid()))) break;
                    _peers.TryAdd(incommingPeer.GetUuid(), incommingPeer);
                    Console.WriteLine("Peer added: " + incommingPeer.GetUuid());
                }
            }
        }

        public void SaveFile(){
            var json = JsonConvert.SerializeObject(_peers);
            if (_path == null) return;
            using (var fileStream = _hiddenPath.WriteToFile(_peerFilePath)){
                var jsonIndex = new UTF8Encoding(true).GetBytes(json);
                fileStream.Write(jsonIndex, 0, jsonIndex.Length);
            }
        }

        public bool Load(){
            if (_peerFilePath != null && File.Exists(this._peerFilePath)){
                string json = File.ReadAllText(this._peerFilePath ?? throw new NullReferenceException());
                ConcurrentDictionary<string, Peer> input =
                    JsonConvert.DeserializeObject<ConcurrentDictionary<string, Peer>>(json);
                foreach (var peer in input){
                    _peers.TryAdd(peer.Value.GetUuid(), peer.Value);
                }

                return true;
            }

            return false;
        }

        private bool InPeerList(string uuid, ConcurrentDictionary<string, Peer> input){
            bool inPeers = false;

            foreach (var peer in input){
                if (peer.Value.GetUuid().Equals(uuid)){
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

                if (DiskHelper.GetTotalAvailableSpace("C:\\") > uploadMessage.filesize){
                    uploadMessage.statusCode = StatusCode.ACCEPTED;
                    Console.WriteLine(@"Request accepted");
                } else{
                    Console.WriteLine(@"Not enough space");
                    uploadMessage.statusCode = StatusCode.INSUFFICIENT_STORAGE;
                }

                uploadMessage.CreateReply();
                uploadMessage.port = _ports.GetAvailablePort();

                _fileReceiver = new FileReceiver(this._path + @".hidden\" + uuid + @"\" + uploadMessage.filehash + @"\",
                    uploadMessage.filename,
                    uploadMessage.port,
                    true);
                _fileReceiver.Start();

                uploadMessage.Send(replyPort);
            }
        }

        private void ReceivedPing(PingMessage ping){
            // Update peer
            foreach (var peer in _peers){
                if (peer.Value.GetUuid().Equals(ping.fromUuid)){
                    peer.Value.SetIp(ping.from);
                    peer.Value.UpdateLastSeen();
                    peer.Value.SetOnline(true);
                    peer.Value.diskSpace = ping.diskSpace;
                    if(ping.type.Equals(TypeCode.RESPONSE))
                        peer.Value.AddPingToList(ping.GetElapsedTime());
                }
            }

            // Respond to ping
            if (ping.type.Equals(TypeCode.REQUEST)){
                ping.CreateReply();
                ping.statusCode = StatusCode.OK;
                ping.diskSpace = DiskHelper.GetTotalAvailableSpace(_path);
                ping.Send();
            } else{
                // Received response, should send peerlist
                PeerFetcherMessage peerFetch = new PeerFetcherMessage(GetAPeer(ping.fromUuid));
                peerFetch.peers = this.GetPeerList();
                // Removed the receiver from the list, as he should not add himself
                foreach (Peer peer in peerFetch.peers){
                    if (string.Compare(peer.GetIp().Trim(), peerFetch.to.Trim(), StringComparison.Ordinal) == 0){
                        peerFetch.peers.Remove(peer);
                        break;
                    }
                }

                peerFetch.Send();
            }
        }

        private Peer GetAPeer(string uuid){
            foreach (var peer in _peers){
                if (peer.Value.UUID.Equals(uuid)){
                    return peer.Value;
                }
            }

            return null;
        }

        private void ReceivedDownloadMessage(DownloadMessage downloadMessage){
            if (downloadMessage.type.Equals(TypeCode.REQUEST)){
                string path = _path + @".hidden\" + downloadMessage.fromUuid + @"\" +
                              downloadMessage.fullFileName + @"\" + downloadMessage.filehash;
                if (downloadMessage.statusCode == StatusCode.OK){
                    Console.WriteLine(path);
                    if (File.Exists(path)){
                        downloadMessage.CreateReply();
                        downloadMessage.statusCode = StatusCode.ACCEPTED;
                        downloadMessage.Send(downloadMessage.port);
                        Console.WriteLine("Response send");
                    } else{
                        Console.WriteLine("File not found");
                        downloadMessage.CreateReply();
                        downloadMessage.statusCode = StatusCode.FILE_NOT_FOUND;
                        downloadMessage.Send(downloadMessage.port);
                    }
                } else if (downloadMessage.statusCode.Equals(StatusCode.ACCEPTED)){
                    var sender = new FileSender(downloadMessage.from, downloadMessage.port);
                    sender.Send(path);
                    Console.WriteLine("File send");
                }
            }
        }

        private void ReceivedDeletionRequest(FileDeletionMessage message){
            Console.WriteLine("Deletion Message Received.");
            if (message.type.Equals(TypeCode.REQUEST)){
                if (message.statusCode.Equals(StatusCode.OK)){
                    string path = _path + @".hidden\" + message.fromUuid + @"\" + message.fullFileHash + @"\" +
                                  message.fileHash;
                    Console.WriteLine(path);
                    if (File.Exists(path)){
                        File.Delete(path);
                        
                        message.statusCode = StatusCode.ACCEPTED;
                        message.CreateReply();
                        message.Send();
                    } else{
                        message.statusCode = StatusCode.FILE_NOT_FOUND;
                        message.CreateReply();
                        message.Send();
                    }
                }
            } else if (message.type.Equals((TypeCode.RESPONSE))){
                if (message.statusCode.Equals(StatusCode.OK)){
                    
                    List<string> updatedList = _locationDb[message.fileHash];
                    updatedList.Remove(message.fromUuid);
                    
                    if (updatedList.Count == 0){
                        _locationDb.TryRemove(message.fileHash, out _);
                    } else{
                        _locationDb.TryAdd(message.fileHash,updatedList);
                    }
                } else if (message.statusCode.Equals(StatusCode.FILE_NOT_FOUND)){
                    List<string> updatedList = _locationDb[message.fileHash];
                    updatedList.Remove(message.fromUuid);
                    if (updatedList.Count == 0){
                        _locationDb.TryRemove(message.fileHash, out _);
                    } else{
                        _locationDb.TryAdd(message.fileHash,updatedList);
                    }

                    Console.WriteLine("File not found at peer");
                }
            }
        }

        public void Stop(){
            _pingTimer.Enabled = false;
            foreach (var manager in _managers){
                manager.Shutdown();
            }

            _hashList.Save();
            _upload.Save(_path + @".hidden\uploadQueue.json");
            _download.Save(_path + @".hidden\downloadQueue.json");
            _deletionQueue.Save(_path + @".hidden\deletionQueue.json");

            this._running = false;
            _receive.Stop();
            SaveLocationDb();
        }

        private void LoadLocationDb(){
            if (File.Exists(_locationDbPath)){
                _locationDb =
                    JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<string>>>(
                        File.ReadAllText(_locationDbPath));
            } else{
                _locationDb = new ConcurrentDictionary<string, List<string>>();
            }
        }

        private void SaveLocationDb(){
            var json = JsonConvert.SerializeObject(_locationDb);
            if (_path == null) return;
            using (var fileStream = _hiddenPath.WriteToFile(_locationDbPath)){
                var jsonIndex = new UTF8Encoding(true).GetBytes(json);
                fileStream.Write(jsonIndex, 0, jsonIndex.Length);
            }
        }

        public void UploadFile(string hash, string path, int copies){
            this._upload.Enqueue(new QueuedFile(hash, path, copies));
        }

        public void DownloadFile(string hash){
            this._download.Enqueue(new QueuedFile(hash));
        }

        public void DeleteFile(string hash){
            this._deletionQueue.Enqueue(hash);
        }

        public void UpdateTopPeers() {
            List<Peer> topPeers = _peers.Values.Where(peer => peer.IsOnline() == true).ToList<Peer>();
            topPeers.Sort(new ComparePeersByRating());
            Console.WriteLine($"_peers:{_peers.Count}       topPeers: {topPeers.Count}");
            if (topPeers.Count > 0) {
                topPeers.RemoveRange(Math.Min(_numberOfPrimaryPeers, topPeers.Count), Math.Max(0, topPeers.Count - _numberOfPrimaryPeers));
                foreach (Peer peer in topPeers) {
                    Console.WriteLine(peer.Rating);
                }
            }
        }
    }
}