using P2P_lib.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace P2P_lib {
    public class DownloadManager{
        private bool is_running = true;
        private int _port;
        private string filehash;
        private string _path;
        private NetworkPorts _ports;
        private ManualResetEvent _waitHandle;
        private BlockingCollection<Peer> _peers;
        private P2PConcurrentQueue<QueuedFile> _queue;
        private FileReceiver _receiver;

        public DownloadManager(P2PConcurrentQueue<QueuedFile> queue, NetworkPorts ports,
            BlockingCollection<Peer> peers){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
            this._path = DiskHelper.getRegistryValue("Path").ToString();
            this._waitHandle = new ManualResetEvent(false);
            this._queue.FileAddedToQueue += _queue_FileAddedToQueue;
            this._port = _ports.GetAvailablePort();
        }

        private void _queue_FileAddedToQueue(){
            this._waitHandle.Set();
        }

        public void Run(){
            while (is_running){

                this._waitHandle.WaitOne();

                QueuedFile file;

                while (this._queue.TryDequeue(out file)){
                    Console.WriteLine("Trying to deque");
                    List<Peer> onlinePeers = this.getPeers();
                    foreach (var peer in _peers){
                        if (peer.isOnline()){
                            onlinePeers.Add(peer);
                        }
                    }

                    filehash = file.GetHash();
                    _ports.Release(_port);

                    Receiver receiver = new Receiver(_port);
                    receiver.MessageReceived += _receiver_MessageReceived;
                    receiver.start();

                    foreach (var onlinePeer in onlinePeers){
                        DownloadMessage downloadMessage = new DownloadMessage(onlinePeer);
                        downloadMessage.port = _port;
                        downloadMessage.filehash = file.GetHash();
                        downloadMessage.filesize = file.GetFilesize();
                        downloadMessage.Send();
                    }

                    //FileReceiver receiver = new FileReceiver();
                    
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
                        download.CreateReply();
                        download.type = Messages.TypeCode.REQUEST;
                        download.statuscode = StatusCode.ACCEPTED;
                        download.port = _ports.GetAvailablePort();
                        var fileReceiver = new FileReceiver(_path, download.filehash + ".aes", download.port, false);
                        fileReceiver.start();
                        Console.WriteLine("FileReceiver opened");
                        download.Send();
                        _ports.Release(_port);
                    } else if (download.statuscode == StatusCode.FILE_NOT_FOUND) {
                        //TODO Responed with FILE_NOT_FOUND
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
