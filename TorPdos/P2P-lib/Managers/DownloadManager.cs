using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Compression;
using Encryption;
using Index_lib;
using P2P_lib.Messages;

namespace P2P_lib.Managers{
    public class DownloadManager : Manager{
        private bool is_running = true;
        private int _port;
        private string _filehash;
        private string _path;
        private NetworkPorts _ports;
        private ManualResetEvent _waitHandle;
        private ConcurrentDictionary<string, Peer> _peers;
        private StateSaveConcurrentQueue<QueuedFile> _queue;
        private FileReceiver _receiver;
        private Index _index;
        public bool isStopped;

        private ConcurrentDictionary<string, List<string>> _sentTo;

        public ConcurrentDictionary<string, List<string>> sentTo
        {
            get { return _sentTo; }
            set
            {
                if (_sentTo == null) _sentTo = value;
            }
        }

        public DownloadManager(StateSaveConcurrentQueue<QueuedFile> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, Index index){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
            this._path = DiskHelper.getRegistryValue("Path").ToString();
            this._waitHandle = new ManualResetEvent(false);
            this._index = index;
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            this._port = _ports.GetAvailablePort();
        }

        private void QueueElementAddedToQueue(){
            this._waitHandle.Set();
        }

        public void Run(){
            isStopped = false;
            while (is_running){
                this._waitHandle.WaitOne();

                QueuedFile file;

                while (this._queue.TryDequeue(out file)){

                    if(!is_running){
                        this._queue.Enqueue(file);
                        break;
                    }

                    _filehash = file.GetHash();
                    if (!_sentTo.ContainsKey(_filehash)) {
                        this._queue.Enqueue(file);
                        Console.WriteLine("File not on network");
                        continue;
                    }

                    List<Peer> onlinePeers = this.GetPeers();

                    if (onlinePeers.Count == 0){
                        _queue.Enqueue(file);
                        this._waitHandle.Reset();
                        continue;
                    }
                    //See if any online peers have the file
                    List<string> sentToPeers = new List<string>();
                    _sentTo.TryGetValue(_filehash, out sentToPeers);
                    onlinePeers = OnlinePeersWithFile(onlinePeers, sentToPeers);
                    if(onlinePeers.Count == 0) {
                        Console.WriteLine("No online peers with file");
                        _queue.Enqueue(file);
                        this._waitHandle.Reset();
                        continue;
                    }

                    _ports.Release(_port);

                    Receiver receiver = new Receiver(_port);
                    receiver.MessageReceived += _receiver_MessageReceived;
                    receiver.Start();

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
            isStopped = true;
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
                        this._receiver =
                            new FileReceiver(Directory.CreateDirectory(_path + @".hidden\" + @"incoming\").FullName,
                                download.filehash + ".aes", download.port, false);
                        this._receiver.FileSuccefullyDownloaded += _receiver_fileSuccefullyDownloaded;
                        this._receiver.Start();
                        Console.WriteLine("FileReceiver opened");
                        download.Send();
                        _ports.Release(_port);
                    } else if (download.statuscode == StatusCode.FILE_NOT_FOUND){
                        //TODO Responed with FILE_NOT_FOUND
                    }
                }
            }
        }

        private void _receiver_fileSuccefullyDownloaded(string path){
            Console.WriteLine("File downloaded");
            RestoreOriginalFile(path);
        }

        private List<Peer> GetPeers(){
            List<Peer> availablePeers = new List<Peer>();

            foreach(var peer in this._peers){
                if (peer.Value.IsOnline())
                {
                    availablePeers.Add(peer.Value);
                }
            }

            return availablePeers;
        }

        private List<Peer> OnlinePeersWithFile(List<Peer> peerlist, List<string> hasFile)
        {
            List<Peer> result = new List<Peer>();
            foreach(Peer peer in peerlist) {
                if (hasFile.Contains(peer.UUID))
                    result.Add(peer);
            }
            return result;
        }

        private void RestoreOriginalFile(string path){
            if (File.Exists(path)){
                Console.WriteLine("File exist");
                string pathWithoutExtension = (_path + @".hidden\incoming\" + Path.GetFileNameWithoutExtension(path));

                // Decrypt file
                FileEncryption decryption = new FileEncryption(pathWithoutExtension, ".lzma");
                decryption.DoDecrypt("password");
                Console.WriteLine("File decrypted");
                File.Delete(path);
                Console.WriteLine(pathWithoutExtension);

                // Decompress file
                string pathToFileForCopying =
                    ByteCompressor.DecompressFile(pathWithoutExtension + ".lzma", pathWithoutExtension);
                Console.WriteLine("File decompressed");
                foreach (string filePath in _index.GetEntry(Path.GetFileNameWithoutExtension(path)).paths){
                    File.Copy(pathToFileForCopying, filePath);
                    Console.WriteLine("File send to: {0}", filePath);
                }
            }
        }

        public override bool Shutdown(){
            is_running = false;
            _waitHandle.Set();

            Console.Write("Download thread stopping... ");
            while(!this.isStopped){}
            Console.Write("Stopped!");

            return true;
        }
    }
}