using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using P2P_lib.Messages;
using P2P_lib;
using System.IO;

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
            this._path = DiskHelper.getRegistryValue("Path").ToString() + @".hidden\";
            this._waitHandle = new ManualResetEvent(false);
            this._queue.FileAddedToQueue += _queue_FileAddedToQueue;
        }

        private void _queue_FileAddedToQueue(){
            this._waitHandle.Set();
        }

        public void Run(){
            while (is_running){
                
                int port = _ports.GetAvailablePort();
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


                    Receiver receiver = new Receiver(port);
                    receiver.MessageReceived += _receiver_MessageReceived;
                    receiver.start();


                    foreach (var onlinePeer in onlinePeers){
                        DownloadMessage downloadMessage = new DownloadMessage(onlinePeer);
                        downloadMessage.port = port;
                        downloadMessage.filehash = file.GetHash();
                        downloadMessage.filesize = file.GetFilesize();
                        downloadMessage.Send();
                    }

                    //FileReceiver receiver = new FileReceiver();
                    _ports.Release(port);
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
                        var fileReceiver = new FileReceiver(_path + @"\.hidden\", download.filehash, download.port, true);
                        fileReceiver.start();
                        download.Send();
                    } else if (download.statuscode == StatusCode.FILE_NOT_FOUND) {
                        //TODO Responed with FILE_NOT_FOUND
                    }
                } else if (download.type.Equals(Messages.TypeCode.REQUEST)) {
                    string pathToFile = (_path + download.FromUUID + download.filehash + @".aes");
                    if (download.statuscode == StatusCode.OK) {
                        if (File.Exists(pathToFile)) {
                            download.CreateReply();
                            download.statuscode = StatusCode.ACCEPTED;
                            download.Send(download.port);
                        } else {
                            download.CreateReply();
                            download.statuscode = StatusCode.FILE_NOT_FOUND;
                            download.Send(download.port);
                        }
                    } else if (download.statuscode == StatusCode.ACCEPTED) {
                        var sender = new FileSender(download.from, download.port);
                        sender.Send(pathToFile);
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