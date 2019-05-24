using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Timers;
using Index_lib;
using Newtonsoft.Json;
using P2P_lib.Handlers;
using P2P_lib.Handlers.FileHandlers;
using P2P_lib.Helpers;
using P2P_lib.Managers;
using P2P_lib.Messages;
using Timer = System.Timers.Timer;
using TypeCode = P2P_lib.Messages.TypeCode;

namespace P2P_lib{
    public class Network{
        private const int NumOfThreads = 5;
        private readonly int _port;
        public bool running;
        private readonly Index _index;
        private Receiver _receive;
        private FileReceiver _fileReceiver;
        private readonly string _path;
        private readonly HiddenFolder _hiddenPath;
        private readonly ConcurrentDictionary<string, Peer> _peers = new ConcurrentDictionary<string, Peer>();
        private readonly string _peerFilePath;
        private readonly StateSaveConcurrentQueue<P2PFile> _upload;
        private readonly StateSaveConcurrentQueue<P2PFile> _download;
        private readonly StateSaveConcurrentQueue<string> _deletionQueue;
        private ConcurrentDictionary<string, P2PFile> _filesList = new ConcurrentDictionary<string, P2PFile>();
        private readonly List<Manager> _managers = new List<Manager>();
        private readonly NetworkPorts _ports = new NetworkPorts();
        private Timer _pingTimer;
        private DeletionManager _deletionManager;
        private readonly string _localtionPath;

        /// <summary>
        /// Sets the proporties for the network
        /// </summary>
        /// <param name="port">The port which the network has to start up on</param>
        /// <param name="index">The index in which the files are indexed</param>
        /// <param name="path">The path to the program folder</param>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Network(int port, Index index, string path = "C:\\TorPdos\\"){
            this._port = port;
            this._path = path;
            this._index = index;
            this._peerFilePath = path + @".hidden\peer.json";
            _hiddenPath = new HiddenFolder(_path + @".hidden\");

            LoadPeer();
            _localtionPath = _path + @".hidden\location.json";
            LoadLocation();

            _deletionQueue = StateSaveConcurrentQueue<string>.Load(_path + @".hidden\deletion.json");
            _upload = StateSaveConcurrentQueue<P2PFile>.Load(_path + @".hidden\upload.json");
            _download = StateSaveConcurrentQueue<P2PFile>.Load(_path + @".hidden\download.json");
        }

        /// <summary>
        /// Function to find the peerlist
        /// </summary>
        /// <returns>It returns the peer list</returns>
        public List<Peer> GetPeerList(){
            var newPeerList = new List<Peer>();

            //Adds a peer to the peer list
            foreach (var peer in _peers){
                newPeerList.Add(peer.Value);
            }
            return newPeerList;
        }

        /// <summary>
        /// Starts the actual P2P network
        /// </summary>
        public void Start(){
            this.running = true;

            _receive = new Receiver(this._port);
            _receive.MessageReceived += Receive_MessageReceived;
            _receive.Start();

            _deletionManager = new DeletionManager(_deletionQueue, _ports, _peers, _filesList);
            var deletionManager = new Thread(_deletionManager.Run);
            deletionManager.Start();
            _managers.Add(_deletionManager);

            //For every thread it opens up a new upload and download manager
            for (int i = 0; i < NumOfThreads; i++){
                var uploadManager = new UploadManager(_upload, _ports, _peers);
                var downloadManager = new DownloadManagerV2(_download, _ports, _peers, _index);

                var uploadThread = new Thread(uploadManager.Run);
                var downloadThread = new Thread(downloadManager.Run);

                uploadThread.Start();
                downloadThread.Start();

                _managers.Add(uploadManager);
                _managers.Add(downloadManager);
            }

            _pingTimer = new Timer();
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

        /// <summary>
        /// Pings the peers
        /// </summary>
        public void Ping(){
            long millis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            foreach (var peer in _peers){
                peer.Value.Ping(millis, _path);
            }
        }

        /// <summary>
        /// Adds a peer
        /// </summary>
        /// <param name="uuid">The peer's unique user ID</param>
        /// <param name="ip">The peer's IP</param>
        public void AddPeer(string uuid, string ip){
            var peer = new Peer(uuid, ip);
            this._peers.TryAdd(peer.GetUuid(), peer);
        }

        /// <summary>
        /// Function to receive a message
        /// </summary>
        /// <param name="message"></param>
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

        /// <summary>
        /// Function to receive peers from one or your peers peer list
        /// </summary>
        /// <param name="message">It is the message which is sent to the receiver</param>
        private void ReceivedPeerFetch(PeerFetcherMessage message){
            if (message.type.Equals(TypeCode.REQUEST)){
                var outgoing = new List<Peer>();
                var incoming = message.peers;
                // Adding sender to list
                if (!InPeerList(message.fromUuid, _peers)){
                    _peers.TryAdd(message.fromUuid, new Peer(message.fromUuid, message.from));
                }

                //Checks whether a incomming peer exists in the peerlist.
                foreach (var incomingPeer in incoming){
                    if (InPeerList(incomingPeer.GetUuid(), _peers)) break;
                    _peers.TryAdd(incomingPeer.GetUuid(), incomingPeer);
                    DiskHelper.ConsoleWrite("Peer added: " + incomingPeer.GetUuid());
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
                foreach (Peer incomingPeer in message.peers){
                    if (InPeerList(incomingPeer.GetUuid(), _peers)) break;
                    if ((IdHandler.GetUuid().Equals(incomingPeer.GetUuid()))) break;
                    _peers.TryAdd(incomingPeer.GetUuid(), incomingPeer);
                    Console.WriteLine("Peer added: " + incomingPeer.GetUuid());
                }
            }
        }

        /// <summary>
        /// Saves a peer to the peerlist
        /// </summary>
        public void SavePeer(){
            var json = JsonConvert.SerializeObject(_peers);
            if (_path == null) return;
            using (var fileStream = _hiddenPath.WriteToFile(_peerFilePath)){
                var jsonIndex = new UTF8Encoding(true).GetBytes(json);
                fileStream.Write(jsonIndex, 0, jsonIndex.Length);
            }
        }

        /// <summary>
        /// Loads the peer from peer list
        /// </summary>
        /// <returns>Returns true if there is a peerlist otherwise returns false</returns>
        private void LoadPeer(){
            if (_peerFilePath != null && File.Exists(this._peerFilePath)){
                string json = File.ReadAllText(this._peerFilePath ?? throw new NullReferenceException());
                var input =
                    JsonConvert.DeserializeObject<ConcurrentDictionary<string, Peer>>(json);
                foreach (var peer in input){
                    _peers.TryAdd(peer.Value.GetUuid(), peer.Value);
                }
            }
        }

        /// <summary>
        /// Checks if a peer is already in peer list 
        /// </summary>
        /// <param name="uuid">The user ID of the peer to be checked</param>
        /// <param name="input">The peer list</param>
        /// <returns>Returns true is the peer is in the list</returns>
        private bool InPeerList(string uuid, ConcurrentDictionary<string, Peer> input){
            bool inPeers = false;

            foreach (var peer in input){
                if (peer.Value.GetUuid().Equals(uuid)){
                    inPeers = true;
                    break;
                }
            }
            return inPeers;
        }

        /// <summary>
        /// Checks if a peer is already in peer list 
        /// </summary>
        /// <param name="uuid">The user ID of the peer to be checked</param>
        /// <param name="input">The peer list</param>
        /// <returns>Returns true is the peer is in the list</returns>
        private bool InPeerList(string uuid, List<Peer> input){
            bool inPeers = false;
            foreach (Peer peer in input){
                if (peer.GetUuid().Equals(uuid)){
                    inPeers = true;
                    break;
                }
            }
            return inPeers;
        }

        /// <summary>
        /// Function to receive upload message
        /// </summary>
        /// <param name="uploadMessage">The message to be received</param>
        private void ReceivedUpload(UploadMessage uploadMessage){
            if (uploadMessage.type.Equals(TypeCode.REQUEST)){
                int replyPort = uploadMessage.port;
                string uuid = uploadMessage.fromUuid;

                if (DiskHelper.GetTotalAvailableSpace(@"C:\") > uploadMessage.filesize){
                    uploadMessage.statusCode = StatusCode.ACCEPTED;
                    Console.WriteLine(@"Request accepted");
                } else{
                    Console.WriteLine(@"Not enough space");
                    uploadMessage.statusCode = StatusCode.INSUFFICIENT_STORAGE;
                }
                uploadMessage.CreateReply();
                uploadMessage.port = _ports.GetAvailablePort();

                _fileReceiver =
                    new FileReceiver(this._path + @".hidden\" + uuid + @"\" + uploadMessage.fullFilename + @"\",
                        uploadMessage.chunkHash, uploadMessage.port);
                _fileReceiver.Start();
                uploadMessage.Send(replyPort);
            }
            _ports.Release(uploadMessage.port);
        }

        /// <summary>
        /// Function to receive ping
        /// </summary>
        /// <param name="ping">The ping message to be received</param>
        private void ReceivedPing(PingMessage ping){
            // Update peer
            foreach (var peer in _peers){
                if (peer.Value.GetUuid().Equals(ping.fromUuid)){
                    peer.Value.SetIp(ping.from);
                    peer.Value.UpdateLastSeen();
                    peer.Value.SetOnline(true);
                    peer.Value.diskSpace = ping.diskSpace;
                    if (ping.type.Equals(TypeCode.RESPONSE))
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
                var peerFetch =
                    new PeerFetcherMessage(GetAPeer(ping.fromUuid)){peers = this.GetPeerList()};
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

        /// <summary>
        /// Function to get a peer
        /// </summary>
        /// <param name="uuid">User ID of the peer whose value the fucntion has to get</param>
        /// <returns></returns>
        private Peer GetAPeer(string uuid){
            foreach (var peer in _peers){
                if (peer.Value.UUID.Equals(uuid)){
                    return peer.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Function to receive download message
        /// </summary>
        /// <param name="downloadMessage">The message which has to be received</param>
        private void ReceivedDownloadMessage(DownloadMessage downloadMessage){
            if (downloadMessage.type.Equals(TypeCode.REQUEST)){
                string path = _path + @".hidden\" + downloadMessage.fromUuid + @"\" +
                              downloadMessage.fullFileName + @"\" + downloadMessage.filehash;
                if (downloadMessage.statusCode == StatusCode.OK){
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
                    var sender = new ChunkSender(downloadMessage.from, downloadMessage.port);
                    sender.Send(path);
                    DiskHelper.ConsoleWrite("File send " + downloadMessage.filehash);
                }
            }
        }

        /// <summary>
        /// Function to receive a deletion request of files on the receiver's computer
        /// </summary>
        /// <param name="message">The message with the chunks which has to be deleted</param>
        private void ReceivedDeletionRequest(FileDeletionMessage message){
            DiskHelper.ConsoleWrite("Deletion Message Received.");
            if (!message.type.Equals(TypeCode.REQUEST)) return;
            if (!message.statusCode.Equals(StatusCode.OK)) return;
            string path = _path + @".hidden\" + message.fromUuid + @"\" + message.fullFileHash + @"\" +
                          message.fileHash;
            DiskHelper.ConsoleWrite(path);
            if (File.Exists(path)){
                File.Delete(path);
                message.statusCode = StatusCode.ACCEPTED;
                message.CreateReply();
                message.Send(message.port);
            } else{
                message.statusCode = StatusCode.FILE_NOT_FOUND;
                message.CreateReply();
                message.Send(message.port);
            }
        }

        /// <summary>
        /// Function to stop the P2P network
        /// </summary>
        public void Stop(){
            _pingTimer.Enabled = false;
            foreach (var manager in _managers){
                manager.Shutdown();
            }

            _upload.Save(_path + @".hidden\uploadQueue.json");
            _download.Save(_path + @".hidden\downloadQueue.json");
            _deletionQueue.Save(_path + @".hidden\deletionQueue.json");
            SaveLocationFile();

            this.running = false;
            _receive.Stop();
        }

        /// <summary>
        /// Saves the location file
        /// </summary>
        /// <returns>Returns true</returns>
        private void SaveLocationFile(){
            var settings = new JsonSerializerSettings
                {TypeNameHandling = TypeNameHandling.Objects, Formatting = Formatting.Indented};
            string output = JsonConvert.SerializeObject(_filesList, settings);

            using (var fileStream = new FileStream(_localtionPath, FileMode.Create)){
                byte[] jsonIndex = new UTF8Encoding(true).GetBytes(output);
                fileStream.Write(jsonIndex, 0, jsonIndex.Length);
                fileStream.Close();
            }
        }

        /// <summary>
        /// Function to load a location file where all the chunks are saved and at what peers
        /// </summary>
        /// <returns>Returns if the peer has the file</returns>
        private void LoadLocation(){
            if (_filesList != null && File.Exists(this._localtionPath)){
                var settings = new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Objects};
                string json = File.ReadAllText(this._localtionPath ?? throw new NullReferenceException());
                var input =
                    JsonConvert.DeserializeObject<ConcurrentDictionary<string, P2PFile>>(json, settings);
                _filesList = input;
            }
        }

        /// <summary>
        /// Function to upload a file
        /// </summary>
        /// <param name="file">The file which needs to be uploaded</param>
        public void UploadFile(P2PFile file){
            if (_filesList.ContainsKey(file.hash)){
                _filesList.TryRemove(file.hash, out _);
            }
            _filesList.TryAdd(file.hash, file);

            this._upload.Enqueue(file);
        }

        /// <summary>
        /// Function to download a file from the network
        /// </summary>
        /// <param name="file">The name of the file which needs to be downloaded</param>
        public void DownloadFile(string file){
            _filesList.TryGetValue(file, out var outputFile);
            this._download.Enqueue(outputFile);
        }

        /// <summary>
        /// Funtion to delete a file on the network
        /// </summary>
        /// <param name="hash">The hash of the file which needs to be deleted</param>
        public void DeleteFile(string hash){
            this._deletionQueue.Enqueue(hash);
        }
    }
}