using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using P2P_lib.Messages;

namespace P2P_lib{
    public class Network{
        private int _port;
        private bool _running = false;
        private Thread _pingThread;
        private Receiver receive;
        private FileReceiver fileReceiver;
        BlockingCollection<Peer> peers = new BlockingCollection<Peer>();

        public Network(int port){
            this._port = port;
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

            receive = new Receiver(this._port, 2048);
            receive.MessageReceived += Receive_MessageReceived;
            receive.start();

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
                RechievedDownload((DownloadMessage) message);
            } else if (msgType == typeof(PeerFetcherMessage)){
                RechievedPeerFetch((PeerFetcherMessage) message);
            }
        }

        private void RechievedPeerFetch(PeerFetcherMessage message){
            if (message.type.Equals(Messages.TypeCode.REQUEST)){
                Console.WriteLine("Rechived Peers");
                List<Peer> newPeers = new List<Peer>();
                //Add sender to list
                message.Peers.Add(new Peer(message.FromUUID.Trim() , message.from.Trim()));
                bool inPeers;
                // Clear already know peers from rechieved peerlist
                foreach (Peer myPeer in peers){
                    inPeers = false;
                    foreach (Peer yourPeer in message.Peers){
                        if (String.Compare(myPeer.GetIP().Trim(), yourPeer.GetIP().Trim(), StringComparison.Ordinal) == 0) {

                            message.Peers.Remove(yourPeer);
                            inPeers = true;
                            break;
                        }
                    }
                    // Add unknown peers to return list
                    if (!inPeers){
                        newPeers.Add(myPeer);
                    }
                }
                //Add new peers to peerlist
                foreach (Peer yourPeer in message.Peers){
                    if (String.Compare(NetworkHelper.getLocalIPAddress().Trim(), yourPeer.GetIP().Trim(), StringComparison.Ordinal) != 0) {
                        peers.Add(yourPeer);
                    }
                }
                // Return senders unknown peers
                message.CreateReply();
                message.Peers = newPeers;
                message.Send();
                Console.WriteLine("Send peers back");
            } else{ // Rechieved response
                bool inPeers = false;
                //Add sender to list
                message.Peers.Add(new Peer(message.FromUUID.Trim(), message.from.Trim()));

                // Don't add yourself to your own list TODO(might not be nessesary, as it should not be send)
                foreach (Peer yourPeer in message.Peers){
                    inPeers = false;
                    foreach (Peer myPeer in peers){
                        if (String.Compare(myPeer.GetIP().Trim(), yourPeer.GetIP().Trim(), StringComparison.Ordinal)==0 ||
                            String.Compare(NetworkHelper.getLocalIPAddress().Trim(), yourPeer.GetIP().Trim(), StringComparison.Ordinal) == 0) {
                            inPeers = true;
                            break;
                        }
                    }
                    // Add unknown peers to own list
                    if (!inPeers){
                        peers.Add(yourPeer);
                    }
                }
            }
            // List peers in console. TODO this is for debugging purpossed and should be removed 
            Console.WriteLine("My peers:");
            foreach (Peer peer in peers){
                Console.WriteLine(peer.getUUID() + " : " + peer.GetIP());
            }
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
                int portForReceivingFile = ports.GetAvailablePort();
                Console.WriteLine("Port for receiving the file: " + portForReceivingFile);
                fileReceiver = new FileReceiver(upload.filehash, true, portForReceivingFile);
                fileReceiver.start();
                Console.WriteLine("File receiver started");
                upload.Send(upload.port);
                Console.WriteLine("Upload response send to port: " + upload.port + "on other computer");
            } else {
                Console.WriteLine("This is not an upload request");
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
            } else{ // Recheved response, should send peerlist
                PeerFetcherMessage peerFetch = new PeerFetcherMessage(ping.from);
               
                peerFetch.from = NetworkHelper.getLocalIPAddress();
                peerFetch.Peers = this.getPeerList();

                //Add myself to the list. TODO Insert userID in place of "MyName"
                peerFetch.FromUUID = "MyName";
                //Removed the rechiever from the list, as he should not add himself
                foreach (Peer peer in peerFetch.Peers) {
                    if (String.Compare(peer.GetIP().Trim(), peerFetch.to.Trim(), StringComparison.Ordinal) == 0) {
                        peerFetch.Peers.Remove(peer);
                        break;
                    }
                }
                peerFetch.statuscode = StatusCode.OK;
                peerFetch.type = Messages.TypeCode.REQUEST;
                //peerFetch.Send();
            }
        }

        private void RechievedDownload(DownloadMessage download){
            throw new NotImplementedException();
        }

        public void Stop(){
            this._running = false;
            receive.stop();
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