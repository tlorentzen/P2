using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using Index_lib;
using Microsoft.Win32;
using Compression;
using Encryption;
using P2P_lib.Messages;

namespace P2P_lib
{
    public class UploadManager
    {
        private ManualResetEvent waitHandle;
        private Boolean is_running = true;
        private NetworkPorts _ports;
        private BlockingCollection<Peer> _peers;
        private P2PConcurrentQueue<QueuedFile> _queue;
        private HiddenFolder _hiddenFolder;
        private RegistryKey registry = Registry.CurrentUser.CreateSubKey("TorPdos\\TorPdos\\TorPdos\\1.2.1.1");
        private string _path;
        private Boolean pendingReceiver = true;
        private FileSender sender;
        private Receiver _receiver;

        public UploadManager(P2PConcurrentQueue<QueuedFile> queue, NetworkPorts ports, BlockingCollection<Peer> peers)
        {
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;

            this.waitHandle = new ManualResetEvent(false);
            this._queue.FileAddedToQueue += _queue_FileAddedToQueue;

            this._path = registry.GetValue("Path").ToString();
            _hiddenFolder = new HiddenFolder(this._path + @"\.hidden\");
        }

        private void _queue_FileAddedToQueue()
        {
            this.waitHandle.Set();
        }

        public void Run(){

            while(is_running)
            {
                this.waitHandle.WaitOne();
                
                QueuedFile file;

                while(this._queue.TryDequeue(out file)){

                    int copies = file.GetCopies();
                    string filePath = file.GetPath();
                    string compressedFilePath = this._path + ".hidden\\" + file.GetHash();

                    // Compress file
                    ByteCompressor.CompressFile(filePath, compressedFilePath);

                    // Encrypt file
                    FileEncryption encryption = new FileEncryption(compressedFilePath, ".lzma");
                    encryption.doEncrypt("password");
                    _hiddenFolder.RemoveFile(compressedFilePath + ".lzma");
                    string encryptedFilePath = compressedFilePath + ".aes";

                    // Split
                    // TODO: split file
                   
                    
                    List<Peer> receivingPeers = this.getPeers(Math.Min(copies, this.CountOnlinePeers()));

                    foreach (Peer peer in receivingPeers)
                    {
                        int port = _ports.GetAvailablePort();
                        _receiver = new Receiver(port);
                        _receiver.start();
                        _receiver.MessageReceived += _receiver_MessageReceived;
                        
                        UploadMessage upload = new UploadMessage(peer);
                        upload.filesize = file.GetFilesize();
                        upload.filename = file.GetFilename();
                        upload.filehash = file.GetHash();
                        upload.path = filePath;
                        upload.port = port;
                        upload.Send();
                        
                        while(pendingReceiver){
                            // TODO: timeout???
                        }

                        _receiver.stop();
                        _ports.Release(port);

                        if (sender != null){
                            sender.Send(encryptedFilePath);
                        }
                    }
                }

                this.waitHandle.Reset();
            }
        }

        private void _receiver_MessageReceived(BaseMessage msg)
        {
            if (msg.GetMessageType() == typeof(UploadMessage))
            {
                UploadMessage upload = (UploadMessage)msg;

                if (upload.type.Equals(Messages.TypeCode.RESPONSE))
                {
                    if (upload.statuscode == StatusCode.ACCEPTED)
                    {
                        sender = new FileSender(upload.from, upload.port);
                        pendingReceiver = false;
                    }
                }
            }
        }

        private List<Peer> getPeers(int count)
        {
            List<Peer> availablePeers = new List<Peer>();
            int counter = 1;

            foreach (Peer peer in this._peers)
            {
                if(peer.isOnline()){
                    availablePeers.Add(peer);

                    if(counter.Equals(count)){
                        break;
                    }

                    counter++;
                }
            }

            return availablePeers;
        }

        private int CountOnlinePeers(){

            int counter = 0;

            foreach (Peer peer in this._peers)
            {
                if (peer.isOnline())
                {
                    counter++;
                }
            }

            return counter;
        }
    }
}
