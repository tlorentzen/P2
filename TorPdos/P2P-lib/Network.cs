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
        BlockingCollection<Peer> peers = new BlockingCollection<Peer>();

        public Network(int port){
            this._port = port;
        }

        public List<Peer> getPeerList(){
            return peers.ToList<Peer>();
        }

        public void Start(){
            this._running = true;

            receive = new Receiver(this._port, 2048);
            receive.MessageReceived += Receive_MessageReceived;
            receive.start();

            //_pingThread = new Thread(this.PingHandler);
            //_pingThread.Start();
        }

        public void AddPeer(string uuid, string ip){
            Peer peer = new Peer(uuid, ip);
            this.peers.Add(peer);
            peer.Ping();
        }

        private void Receive_MessageReceived(BaseMessage message)
        {
            Console.WriteLine(message.GetMessageType());

            Type msgType = message.GetMessageType();

            if (msgType == typeof(PingMessage)){
                RechievedPing((PingMessage)message);
                
            } else if (msgType == typeof(UploadMessage)) {
                RechievedUpload((UploadMessage)message);
                
            } else if (msgType == typeof(DownloadMessage)) {
                RechievedDownload((DownloadMessage)message);

            } else if (msgType == typeof(PeerFetcherMessage)) {
                RechievedPeerFetch((PeerFetcherMessage)message);

            } 
        }

        private void RechievedPeerFetch(PeerFetcherMessage message)
        {
            throw new NotImplementedException();
        }

        private void RechievedUpload(UploadMessage upload)
        {
            if (upload.type.Equals(Messages.TypeCode.REQUEST)) {
                if (DiskHelper.GetTotalFreeSpace("C:\\") > upload.filesize) {
                    upload.statuscode = StatusCode.ACCEPTED;
                } else {
                    upload.statuscode = StatusCode.INSUFFICIENT_STORAGE;
                }
                upload.CreateReply();
                upload.Send();
            }
        }

        private void RechievedPing(PingMessage ping)
        {

            foreach (Peer peer in peers) {
                if (peer.GetIP().Equals(ping.from)) {
                    peer.UpdateLastSeen();
                    peer.setOnline(true);
                }
            }

            if (ping.type.Equals(Messages.TypeCode.REQUEST)) {
                ping.CreateReply();
                ping.statuscode = StatusCode.OK;
                ping.Send();
            }
        }

        private void RechievedDownload(DownloadMessage download)
        {
            throw new NotImplementedException();
        }

        public void Stop(){
            this._running = false;
            receive.stop();
        }
        
        private void PingHandler(){
            while(this._running){
                long millis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                foreach (Peer peer in peers){
                    peer.Ping(millis);

                    if(!this._running){
                        break;
                    }
                }
            }

            Console.WriteLine("PingHandler stopped...");
        }

    }
}
