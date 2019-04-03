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
                Console.WriteLine("Rechived Peers");
                List<Peer> newPeers = new List<Peer>();
                bool inPeers = false;
                foreach (Peer myPeer in peers){
                    inPeers = false;
                    foreach (Peer yourPeer in message.Peers){
                        if (myPeer.GetIP() == yourPeer.GetIP()){

                            message.Peers.Remove(yourPeer);
                            inPeers = true;
                            break;
                        }
                    }

                    if (!inPeers){
                        newPeers.Add(myPeer);
                    }
                }

                foreach (Peer yourPeer in message.Peers){
                    if (!(String.Compare(NetworkHelper.getLocalIPAddress(), yourPeer.GetIP(), StringComparison.Ordinal) == 0)) {
                        peers.Add(yourPeer);
                    }
                }

                message.CreateReply();
                message.Peers = newPeers;
                message.Send();
                Console.WriteLine("Send peers back");
            } else{
                bool inPeers = false;
                foreach (Peer yourPeer in message.Peers){
                    inPeers = false;
                    foreach (Peer myPeer in peers){
                        if (String.Compare(myPeer.GetIP(), yourPeer.GetIP(), StringComparison.Ordinal)==0 ||
                            String.Compare(NetworkHelper.getLocalIPAddress(), yourPeer.GetIP(), StringComparison.Ordinal) == 0) {
                            inPeers = true;
                            break;
                        }
                    }

                    if (!inPeers){
                        peers.Add(yourPeer);
                    }
                }
            }

            Console.WriteLine("My peers:");
            foreach (Peer peer in peers){
                Console.WriteLine(peer.getUUID() + " : " + peer.GetIP());
            }
        }

        private void RechievedUpload(UploadMessage upload){
            if (upload.type.Equals(Messages.TypeCode.REQUEST)){
                if (DiskHelper.GetTotalFreeSpace("C:\\") > upload.filesize){
                    upload.statuscode = StatusCode.ACCEPTED;
                } else{
                    upload.statuscode = StatusCode.INSUFFICIENT_STORAGE;
                }

                upload.CreateReply();
                fileReceiver = new FileReceiver(upload.filehash, true, upload.port);
                fileReceiver.start();
                upload.Send();
            }
        }

        private void RechievedPing(PingMessage ping){
            foreach (Peer peer in peers){
                if (peer.GetIP().Equals(ping.from)){
                    peer.UpdateLastSeen();
                    peer.setOnline(true);
                }
            }

            if (ping.type.Equals(Messages.TypeCode.REQUEST)){
                ping.CreateReply();
                ping.statuscode = StatusCode.OK;
                ping.Send();
            } else{
                PeerFetcherMessage peerFetch = new PeerFetcherMessage(ping.from);
                peerFetch.from = NetworkHelper.getLocalIPAddress();
                peerFetch.Peers = this.getPeerList();
                //TODO Insert username in place of "MyName"
                peerFetch.Peers.Add(new Peer("MyName", NetworkHelper.getLocalIPAddress()));
                foreach(Peer peer in peerFetch.Peers) {
                    if (String.Compare(peer.GetIP(), peerFetch.to, StringComparison.Ordinal) == 0) {
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