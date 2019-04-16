using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using P2P_lib.Messages;

namespace P2P_lib{
    public class DownloadManager{
        private ManualResetEvent _waitHandle;
        private bool is_running = true;
        private NetworkPorts _ports;
        private BlockingCollection<Peer> _peers;
        private P2PConcurrentQueue<QueuedFile> _queue;
        private FileReceiver _receiver;
        private string filehash;
        private string _path;

        public DownloadManager(P2PConcurrentQueue<QueuedFile> queue, NetworkPorts ports,
            BlockingCollection<Peer> peers){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;

            this._waitHandle = new ManualResetEvent(false);
            this._queue.FileAddedToQueue += _queue_FileAddedToQueue;
        }

        private void _queue_FileAddedToQueue(){
            this._waitHandle.Set();
        }

        public void Run(){
            while (is_running){

                this._waitHandle.WaitOne();

                QueuedFile file;

                while (this._queue.TryDequeue(out file)){

                    List<Peer> onlinePeers = this.getPeers();
                    foreach (var peer in _peers){
                        if (peer.isOnline()){
                            onlinePeers.Add(peer);
                        }
                    }

                    filehash = file.GetHash();

                    int port = _ports.GetAvailablePort();

                    Receiver receiver = new Receiver(port);
                    receiver.MessageReceived += _receiver_MessageReceived;
                    receiver.start();

                    foreach (var onlinePeer in onlinePeers){
                        DownloadMessage downloadMessage = new DownloadMessage(onlinePeer);
                        downloadMessage.filehash = file.GetHash();
                        downloadMessage.filesize = file.GetFilesize();
                        downloadMessage.port = port;
                        downloadMessage.Send();
                    }

                    //FileReceiver receiver = new FileReceiver();
                    //_ports.Release(port);

                    Console.WriteLine("File: " + file.GetHash() + " was process in download manager");
                }

                this._waitHandle.Reset();
            }
        }

        private void _receiver_MessageReceived(BaseMessage msg){
            if (msg.GetMessageType() == typeof(DownloadMessage)){
                DownloadMessage download = (DownloadMessage) msg;

                if (download.type.Equals(Messages.TypeCode.RESPONSE)){
                    if (download.statuscode == StatusCode.ACCEPTED){
                        _receiver = new FileReceiver(_path, _path + filehash + ".aes", download.port, false);
                    }
                }
            }
        }

        private List<Peer> getPeers(){
            List<Peer> availablePeers = new List<Peer>();

            foreach (Peer peer in this._peers){
                if (peer.isOnline()){
                    availablePeers.Add(peer);
                }
            }

            return availablePeers;
        }
    }
}