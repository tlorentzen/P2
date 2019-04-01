using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using P2P_lib.Messages;

namespace P2P_lib
{
    public class Network
    {
        private int _port;
        private Boolean _running = false;
        private Thread _pingThread;
        private Receiver receive;
        BlockingCollection<Peer> peers = new BlockingCollection<Peer>();

        public Network(int port){
            this._port = port;
        }

        public List<Peer> getPeerList()
        {
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
        }

        private void Receive_MessageReceived(BaseMessage message)
        {
            Console.WriteLine(message.GetMessageType());

            if (message.GetMessageType() == typeof(PingMessage)){
                PingMessage ping = (PingMessage)message;

                foreach (Peer peer in peers)
                {
                    if(peer.GetIP().Equals(ping.from)){
                        peer.UpdateLastSeen();
                        peer.setOnline(true);
                    }
                }

                if(message.type.Equals(Messages.TypeCode.REQUEST)){
                    ping.reply();
                }
            }
        }

        public void Stop(){
            this._running = false;
            receive.stop();
        }
        
        private void PingHandler()
        {
            while(this._running)
            {
                long millis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                foreach (Peer peer in peers)
                {
                    peer.Ping(millis);

                    if(!this._running)
                    {
                        break;
                    }
                }
            }

            Console.WriteLine("PingHandler stopped...");
        }
    
    }
}
