using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Index_lib;
using Microsoft.Win32;
using Compression;
using Encryption;
using NLog;
using P2P_lib.Messages;

namespace P2P_lib
{
    public class UploadManager
    {
        private ManualResetEvent waitHandle;
        private bool is_running = true;
        private NetworkPorts _ports;
        private BlockingCollection<Peer> _peers;
        private P2PConcurrentQueue<QueuedFile> _queue;
        private HiddenFolder _hiddenFolder;
        private RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"TorPdos\1.1.1.1");
        private string _path;
        private bool pendingReceiver = true;
        private FileSender sender;
        private Receiver _receiver;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("UploadLogger");

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

                    //Console.WriteLine("Current queued files: "+_queue.Count);

                    int copies = file.GetCopies();
                    string filePath = file.GetPath();
                    string compressedFilePath = this._path + @".hidden\" + file.GetHash();

                    List<Peer> receivingPeers = this.getPeers(Math.Min(copies, this.CountOnlinePeers()));

                    if (receivingPeers.Count == 0)
                    {
                        this.waitHandle.Reset();
                        continue;
                    }

                    // Compress file
                    ByteCompressor.compressFile(filePath, compressedFilePath);

                    // Encrypt file
                    FileEncryption encryption = new FileEncryption(compressedFilePath, ".lzma");
                    encryption.doEncrypt("password");
                    //_hiddenFolder.removeFile(compressedFilePath + ".lzma");
                    string encryptedFilePath = compressedFilePath + ".aes";

                    string filename = file.GetHash() + ".aes";
                    

                    // Split
                    // TODO: split file

                    foreach (Peer peer in receivingPeers)
                    {
                        int port = _ports.GetAvailablePort();
                        try{
                            _receiver = new Receiver(port);
                            _receiver.MessageReceived += this._receiver_MessageReceived;
                            _receiver.start();
                        }
                        catch (SocketException e){
                            logger.Log(LogLevel.Fatal, e);
                        }
                        catch (Exception e){
                            logger.Warn(e);
                        }

                        UploadMessage upload = new UploadMessage(peer);
                        upload.filesize = file.GetFilesize();
                        upload.filename = filename;
                        upload.filehash = file.GetHash();
                        upload.path = filePath;
                        upload.port = port;
                        upload.Send();
                        
                        while(pendingReceiver){
                            // TODO: timeout???
                        }

                        _receiver.stop();
                        //_ports.Release(port);

                        if (sender != null){
                            sender.Send(encryptedFilePath);
                        }

                        pendingReceiver = true;
                        _ports.Release(port);
                    }

                    //_hiddenFolder.removeFile(encryptedFilePath);
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
