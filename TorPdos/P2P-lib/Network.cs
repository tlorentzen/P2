using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Permissions;
using System.Threading;
using Index_lib;
using Newtonsoft.Json;
using P2P_lib.Messages;
using Index_lib;
using P2P_lib.Managers;

namespace P2P_lib{
    public class Network{
        private int _numOfThreads = 5;
        private int _port;
        private bool _running = false;
        private Index _index;
        private Thread _pingThread;
        private Receiver _receive;
        private FileReceiver _fileReceiver;
        private string _path;
        private HiddenFolder _hiddenPath;
        private BlockingCollection<Peer> peers = new BlockingCollection<Peer>();
        private string _peerFilePath;
        private Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private P2PConcurrentQueue<QueuedFile> upload = new P2PConcurrentQueue<QueuedFile>();
        private P2PConcurrentQueue<QueuedFile> download = new P2PConcurrentQueue<QueuedFile>();
        private List<Thread> threads = new List<Thread>();
        private NetworkPorts ports = new NetworkPorts();

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Network(int port, Index index, string path = "C:\\TorPdos\\"){
            this._port = port;
            this._path = path;
            this._index = index;
            this._peerFilePath = path + @".hidden\peer.json";
            _hiddenPath = new HiddenFolder(_path + @"\.hidden\");
            load();
        }

        public List<Peer> getPeerList(){
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
            _receive.start();

            _pingThread = new Thread(this.PingHandler);
            _pingThread.Start();

            for (int i = 0; i < _numOfThreads; i++)
            {
                UploadManager uploadmanager = new UploadManager(upload, ports, peers);
                DownloadManager downloadmanager = new DownloadManager(download, ports, peers);

                Thread uploadThread = new Thread(new ThreadStart(uploadmanager.Run));
                Thread downloadThread = new Thread(new ThreadStart(downloadmanager.Run));

                uploadThread.Start();
                downloadThread.Start();

                threads.Add(uploadThread);
                threads.Add(downloadThread);
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
                RechievedUpload((UploadMessage) message);
            } else if (msgType == typeof(DownloadMessage)){
                receivedDownloadMessage((DownloadMessage) message);
            } else if (msgType == typeof(PeerFetcherMessage)){
                RechievedPeerFetch((PeerFetcherMessage) message);
            }
        }

        private void RechievedPeerFetch(PeerFetcherMessage message){
            if (message.type.Equals(Messages.TypeCode.REQUEST)){
                List<Peer> incomming = new List<Peer>();
                List<Peer> outgoing = new List<Peer>();
                incomming = message.Peers;
                // Adding sender to list
                if (!inPeerList(message.FromUUID, peers)){
                    peers.Add(new Peer(message.FromUUID, message.from));
                }

                //Checks whether a incomming peer exists in the peerlist.
                foreach (var incommingPeer in incomming){
                    if (inPeerList(incommingPeer.getUUID(), peers)) break;
                    peers.Add(incommingPeer);
                    Console.WriteLine("Peer added: " + incommingPeer.getUUID());
                }

                foreach (var outGoingPeer in peers){
                    if (inPeerList(outGoingPeer.getUUID(), incomming)) break;
                    if (outGoingPeer.getUUID() == message.FromUUID) break;
                    outgoing.Add(outGoingPeer);
                }
                message.CreateReply();
                message.Peers = outgoing;
                message.Send();
            } else{
                // Rechieved response

                foreach (Peer incommingPeer in message.Peers){
                    if (inPeerList(incommingPeer.getUUID(), peers)) break;
                    
                    if ((config.AppSettings.Settings["uuid"].Value.Equals(incommingPeer.getUUID()))) break;
                    peers.Add(incommingPeer);
                    Console.WriteLine("Peer added: " + incommingPeer.getUUID());
                }
                
            }

            // List peers in console. TODO this is for debugging purposes and should be removed
            //Console.WriteLine("My peers:");
            foreach (Peer peer in peers){
                //Console.WriteLine(peer.getUUID() + " : " + peer.GetIP());
            }
        }

        private bool inPeerList(string UUID, List<Peer> input){
            bool inPeers = false;
            foreach (Peer peer in input){
                if (peer.getUUID().Equals(UUID)){
                    inPeers = true;
                    break;
                }
            }

            // Add unknown peers to own list
            return inPeers;
        }

        public void saveFile(){
            var json = JsonConvert.SerializeObject(peers);
            if (_path == null) return;
            using (var fileStream = _hiddenPath.WriteToFile(_peerFilePath)){
                var jsonIndex = new UTF8Encoding(true).GetBytes(json);
                fileStream.Write(jsonIndex, 0, jsonIndex.Length);
            }
        }

        public bool load(){
            if (_peerFilePath != null && System.IO.File.Exists(this._peerFilePath)){
                string json = System.IO.File.ReadAllText(this._peerFilePath);
                List<Peer> input = JsonConvert.DeserializeObject<List<Peer>>(json);
                foreach (var peer in input){
                    peers.Add(peer);
                }

                return true;
            }

            return false;
        }

        private bool inPeerList(string UUID, BlockingCollection<Peer> input){
            bool inPeers = false;
            foreach (Peer peer in input){
                if (peer.getUUID().Equals(UUID)){
                    inPeers = true;
                    break;
                }
            }
            // Add unknown peers to own list
            return inPeers;
        }

        private void RechievedUpload(UploadMessage upload){
            if (upload.type.Equals(Messages.TypeCode.REQUEST)){
                int replyPort = upload.port;
                string uuid = upload.FromUUID;

                if (DiskHelper.GetTotalFreeSpace("C:\\") > upload.filesize){
                    upload.statuscode = StatusCode.ACCEPTED;
                    Console.WriteLine("Request accepted");
                } else{
                    Console.WriteLine("Not enough space");
                    upload.statuscode = StatusCode.INSUFFICIENT_STORAGE;
                }
                
                upload.CreateReply();
                upload.port = ports.GetAvailablePort();

                _fileReceiver = new FileReceiver(this._path + "\\.hidden\\" + uuid + "\\", upload.filename, upload.port, true);
                _fileReceiver.start();

                upload.Send(replyPort);
            }
        }

        private void RechievedPing(PingMessage ping){
            // Update peer
            foreach (Peer peer in peers){
                if (peer.getUUID().Equals(ping.FromUUID)){
                    peer.SetIP(ping.from);
                    peer.UpdateLastSeen();
                    peer.setOnline(true);
                }
            }

            // Respond to ping
            if (ping.type.Equals(Messages.TypeCode.REQUEST)){
                ping.CreateReply();
                ping.statuscode = StatusCode.OK;
                ping.Send();
            } else{
                // Recheved response, should send peerlist
                PeerFetcherMessage peerFetch = new PeerFetcherMessage(getAPeer(ping.FromUUID));
                peerFetch.Peers = this.getPeerList();
                //Removed the rechiever from the list, as he should not add himself
                foreach (Peer peer in peerFetch.Peers){
                    if (string.Compare(peer.GetIP().Trim(), peerFetch.to.Trim(), StringComparison.Ordinal) == 0){
                        peerFetch.Peers.Remove(peer);
                        break;
                    }
                }
                peerFetch.Send();
            }
        }

        public Peer getAPeer(string UUID){
            foreach (var peer in peers){
                if (peer.UUID.Equals(UUID)){
                    return peer;
                }
            }
            return null;
        }

        private void receivedDownloadMessage(DownloadMessage download){
            if (download.type.Equals(Messages.TypeCode.REQUEST)){
                if (File.Exists(_path + @"\.hidden\" + download.filehash)){
                    download.CreateReply();
                    download.statuscode = StatusCode.ACCEPTED;
                    download.Send();
                } else{
                    download.CreateReply();
                    download.statuscode = StatusCode.ERROR;
                    download.Send();

                    foreach (var peer in peers){
                        download.forwardMessage(peer.GetIP());
                    }
                }
            }
        }

        public void Stop(){

            foreach (Thread thread in threads)
            {
                // TODO: Stop threads 
            }

            this._running = false;
            _receive.stop();
        }

        private void PingHandler(){
            while (this._running){
                long millis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                foreach (Peer peer in peers){
                    peer.Ping(millis);

                    if (!this._running){
                        break;
                    }
                }
            }

            Console.WriteLine("PingHandler stopped...");
        }
        public void UploadFileToNetwork(string filePath, int copies, int seed = 0) {
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
