﻿using System;
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
                Console.WriteLine("Receiving files");
                List<Peer> incomming = new List<Peer>();
                List<Peer> outgoing = new List<Peer>();
                incomming = message.Peers;
                // Adding sender to list
                if (!inPeerList(message.FromUUID,peers)){
                    peers.Add(new Peer(message.FromUUID, message.from));
                }

                //Checks whether a incomming peer exists in the peerlist.
                foreach (var incommingPeer in incomming){
                    if (inPeerList(incommingPeer.getUUID(),peers)) return;
                    peers.Add(incommingPeer);
                }

                foreach (var outGoingPeer in peers){
                    if (inPeerList(outGoingPeer.getUUID(),incomming)) return;
                    if (outGoingPeer.getUUID() == message.FromUUID) return;
                    outgoing.Add(outGoingPeer);
                }

                message.CreateReply();
                message.Peers = outgoing;
                message.Send();


            } else{
                // Rechieved response
                //Add sender to list
                message.Peers.Add(new Peer(message.FromUUID.Trim(), message.from.Trim()));

                // Don't add yourself to your own list
                // TODO(might not be nessesary, as it should not be send)
                foreach (Peer incommingPeer in message.Peers){
                    if (inPeerList(incommingPeer.getUUID(), peers)) return;
                        peers.Add(incommingPeer);
                }
            }

            // List peers in console. TODO this is for debugging purpossed and should be removed 
            Console.WriteLine("My peers:");
            foreach (Peer peer in peers){
                Console.WriteLine(peer.getUUID() + " : " + peer.GetIP());
            }
        }

        private bool inPeerList(string UUID,List<Peer> input){
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
        private bool inPeerList(string UUID,BlockingCollection<Peer> input){
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
                if (DiskHelper.GetTotalFreeSpace("C:\\") > upload.filesize){
                    upload.statuscode = StatusCode.ACCEPTED;
                } else{
                    upload.statuscode = StatusCode.INSUFFICIENT_STORAGE;
                }

                NetworkPorts ports = new NetworkPorts();
                upload.CreateReply();
                upload.port = ports.GetAvailablePort();
                fileReceiver = new FileReceiver(upload.filehash, true, upload.port);
                fileReceiver.start();
                upload.Send();
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
                peerFetch.FromUUID = "MyName"+NetworkHelper.getLocalIPAddress();
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