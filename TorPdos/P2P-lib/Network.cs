using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Security.Permissions;
using System.Threading;
using Index_lib;
using Newtonsoft.Json;
using P2P_lib.Messages;
using Index_lib;

namespace P2P_lib{
    public class Network{
        private int _port;
        private bool _running = false;
        private Index _index;
        private Thread _pingThread;
        private Receiver _receive;
        private FileReceiver _fileReceiver;
        private string _path;
        private HiddenFolder _hiddenPath;
        BlockingCollection<Peer> peers = new BlockingCollection<Peer>();
        private string _peerFilePath = @"C:\\TorPdos\.hidden\peer.json";

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Network(int port, Index index, string path = "C:\\TorPdos\\"){
            this._port = port;
            this._path = path;
            this._index = index;
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

            _receive = new Receiver(this._port, 2048);
            _receive.MessageReceived += Receive_MessageReceived;
            _receive.start();

            _pingThread = new Thread(this.PingHandler);
            _pingThread.Start();
        }

        public void AddPeer(string uuid, string ip){
            Peer peer = new Peer(uuid, ip);
            this.peers.Add(peer);
        }

        private void Receive_MessageReceived(BaseMessage message){
            Console.WriteLine(message.GetMessageType());

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
                Console.WriteLine("Receiving peers");
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
                    Console.WriteLine("Peer added " + incommingPeer.getUUID());
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
                    if (("MyName" + NetworkHelper.getLocalIPAddress()).Equals(incommingPeer.getUUID())) break;
                    peers.Add(incommingPeer);
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
            if (_peerFilePath != null && File.Exists(this._peerFilePath)){
                string json = File.ReadAllText(this._peerFilePath);
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
                Console.WriteLine("This is an upload request");
                if (DiskHelper.GetTotalFreeSpace("C:\\") > upload.filesize){
                    upload.statuscode = StatusCode.ACCEPTED;
                    Console.WriteLine("Request accepted");
                } else{
                    Console.WriteLine("Not enough space");
                    upload.statuscode = StatusCode.INSUFFICIENT_STORAGE;
                }

                upload.CreateReply();
                NetworkPorts ports = new NetworkPorts();
                upload.port = ports.GetAvailablePort();
                Console.WriteLine("Port for receiving the file: " + upload.port);
                _fileReceiver = new FileReceiver(upload.filehash, true, upload.port);
                _fileReceiver.start();
                Console.WriteLine("File receiver started");
                upload.Send();
                Console.WriteLine("Upload response send to: " + upload.to);
            } else if (upload.type.Equals(Messages.TypeCode.RESPONSE)) {
                Console.WriteLine("This is an upload response");
                if (upload.statuscode == StatusCode.ACCEPTED) {
                    Console.WriteLine("It's accepted");
                    Console.WriteLine(upload.filehash.Equals("c28ef56e6bbcdd8ed452cbb860e620d1"));
                    IndexFile indexFile = _index.GetEntry(upload.filehash);
                    string filePath = indexFile.getPath();
                    Console.WriteLine("Filepath to upload: {0}", filePath);
                    Console.WriteLine("Path from indexfile is: " + indexFile.getPath());
                    FileSender fileSender = new FileSender(upload.from, upload.port);
                    Console.WriteLine("Upload is send from: " + upload.from + " and file vil be sent to port: " + upload.port);
                    fileSender.Send(filePath);
                    Console.WriteLine(filePath + " has been sent to port: " + upload.port + " on IP: " + upload.from);
                    //_hiddenPath.Remove(filePath);
                }
            }
        }

        private void RechievedPing(PingMessage ping){
            // Update peer
            foreach (Peer peer in peers){
                if (peer.GetIP().Equals(ping.from)){
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
                PeerFetcherMessage peerFetch = new PeerFetcherMessage(ping.from);

                peerFetch.from = NetworkHelper.getLocalIPAddress();
                peerFetch.Peers = this.getPeerList();

                //Add myself to the list. TODO Insert userID in place of "MyName"
                peerFetch.FromUUID = "MyName" + NetworkHelper.getLocalIPAddress();
                //Removed the rechiever from the list, as he should not add himself
                foreach (Peer peer in peerFetch.Peers){
                    if (String.Compare(peer.GetIP().Trim(), peerFetch.to.Trim(), StringComparison.Ordinal) == 0){
                        peerFetch.Peers.Remove(peer);
                        break;
                    }
                }

                peerFetch.statuscode = StatusCode.OK;
                peerFetch.type = Messages.TypeCode.REQUEST;
                peerFetch.Send();
            }
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
                    download.from = NetworkHelper.getLocalIPAddress();
                    download.Send();

                    foreach (var peer in peers){
                        download.forwardMessage(peer.GetIP());
                    }
                }
            }
        }

        public void Stop(){
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
    }
}
