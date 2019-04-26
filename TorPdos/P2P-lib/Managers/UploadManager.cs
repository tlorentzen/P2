using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Compression;
using Encryption;
using Index_lib;
using Microsoft.Win32;
using NLog;
using P2P_lib.Messages;

namespace P2P_lib.Managers
{
    public class UploadManager
    {
        private ManualResetEvent waitHandle;
        private bool is_running = true;
        private NetworkPorts _ports;
        private BlockingCollection<Peer> _peers;
        private StateSaveConcurrentQueue<QueuedFile> _queue;
        private RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"TorPdos\1.1.1.1");
        private string _path;
        private bool _pendingReceiver = true;
        private FileSender _sender;
        private Receiver _receiver;
        private static Logger logger = LogManager.GetLogger("UploadLogger");
        private HiddenFolder _hiddenFolder;

        public UploadManager(StateSaveConcurrentQueue<QueuedFile> queue, NetworkPorts ports, BlockingCollection<Peer> peers)
        {
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;

            this.waitHandle = new ManualResetEvent(false);
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;

            this._path = registry.GetValue("Path").ToString();
            _hiddenFolder = new HiddenFolder(this._path + @"\.hidden\");
        }

        private void QueueElementAddedToQueue()
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

                    List<Peer> receivingPeers = this.GetPeers(Math.Min(copies, this.CountOnlinePeers()));

                    if (receivingPeers.Count == 0)
                    {
                        this.waitHandle.Reset();
                        continue;
                    }

                    // Compress file
                    ByteCompressor.CompressFile(filePath, compressedFilePath);

                    // Encrypt file
                    FileEncryption encryption = new FileEncryption(compressedFilePath, ".lzma");
                    encryption.DoEncrypt("password");
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
                            _receiver.Start();
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
                        
                        while(_pendingReceiver){
                            // TODO: timeout???
                        }

                        _receiver.Stop();
                        //_ports.Release(port);

                        if (_sender != null){
                            _sender.Send(encryptedFilePath);
                        }

                        _pendingReceiver = true;
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
                        _sender = new FileSender(upload.from, upload.port);
                        _pendingReceiver = false;
                    }
                }
            }
        }

        private List<Peer> GetPeers(int count)
        {
            List<Peer> availablePeers = new List<Peer>();
            int counter = 1;

            foreach (Peer peer in this._peers)
            {
                if(peer.IsOnline()){
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
                if (peer.IsOnline())
                {
                    counter++;
                }
            }

            return counter;
        }
    }
}
