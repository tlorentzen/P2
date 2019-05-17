using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Threading;
using Compression;
using Encryption;
using Index_lib;
using NLog;
using P2P_lib.Messages;
using Splitter_lib;
using System.Linq;
using P2P_lib.Handlers;
using P2P_lib.Helpers;

namespace P2P_lib.Managers{
    public class UploadManager : Manager{
        private readonly ManualResetEvent _waitHandle;
        private bool _isRunning = true;
        private readonly NetworkPorts _ports;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly StateSaveConcurrentQueue<P2PFile> _queue;
        private readonly string _path;
        private bool _pendingReceiver = true;
        private ChunkSender _sender;
        private Receiver _receiver;
        private readonly Logger _logger = LogManager.GetLogger("UploadLogger");
        private bool _isStopped;
        private readonly HiddenFolder _hiddenFolder;


        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public UploadManager(StateSaveConcurrentQueue<P2PFile> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;

            this._waitHandle = new ManualResetEvent(false);
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            _hiddenFolder = new HiddenFolder(_path + @".hidden");

            this._path = DiskHelper.GetRegistryValue("Path");
            Peer.PeerSwitchedOnline += PeerWentOnline;
        }

        private void QueueElementAddedToQueue(){
            this._waitHandle.Set();
        }
        private void PeerWentOnline(){
            this._waitHandle.Set();
        }


        public void Run(){
            _isStopped = false;
            this._waitHandle.Set();

            while (_isRunning){
                this._waitHandle.WaitOne();

                if (!_isRunning)
                    break;
                
                while (this._queue.TryDequeue(out P2PFile file)){

                    bool uploaded = true;

                    if (!_isRunning){
                        this._queue.Enqueue(file);
                        break;
                    }

                    string filePath = file.Paths[0];
                    string compressedFilePath = this._path + @".hidden\" + file.Hash;

                    // Compress file
                    bool compressionCompleted = Compressor.CompressFile(filePath, compressedFilePath);

                    if (!compressionCompleted){
                        this._queue.Enqueue(file);
                        continue;
                    }

                    // Encrypt file
                    var encryption = new FileEncryption(compressedFilePath, ".lzma");

                    bool encryptionCompleted = encryption.DoEncrypt(IdHandler.GetKeyMold());

                    if (!encryptionCompleted){
                        this._queue.Enqueue(file);
                        continue;
                    }

                    _hiddenFolder.Remove(compressedFilePath + ".lzma");

                    string encryptedFilePath = compressedFilePath + ".aes";

                    // Initialize splitter
                    var splitter = new SplitterLibrary(); 

                    List<string> chunks = splitter.SplitFile(encryptedFilePath, file.Hash, _path + @".hidden\splitter\");
                    file.AddChunk(chunks);

                    FileUploader uploader = new FileUploader(_ports, _peers);

                    foreach (var chunk in file.Chunks){
                        string path = _path + @".hidden\splitter\" + chunk.Hash;

                        if(!uploader.Push(chunk, path))
                        {
                            uploaded = false;
                        }
                    }
                
                    if(!uploaded){
                        this._queue.Enqueue(file);
                    }
                }

                this._waitHandle.Reset();
            }

            _isStopped = true;
        }

        public override bool Shutdown(){
            _isRunning = false;
            _waitHandle.Set();

            Console.Write("Upload thread stopping... ");
            while (!this._isStopped){ }

            Console.Write("Stopped!\n");

            return true;
        }
    }
}