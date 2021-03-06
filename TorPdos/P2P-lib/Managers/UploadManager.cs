﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using Compression;
using Encryption;
using Index_lib;
using Splitter_lib;
using P2P_lib.Handlers;
using P2P_lib.Handlers.FileHandlers;
using P2P_lib.Helpers;

namespace P2P_lib.Managers{
    public class UploadManager : Manager{
        private readonly ManualResetEvent _waitHandle;
        private bool _isRunning = true;
        private readonly NetworkPorts _ports;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly StateSaveConcurrentQueue<P2PFile> _queue;
        private readonly string _path;
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

        /// <summary>
        /// Main running function for UploadManager, needs to be called for it to run.
        /// </summary>
        public void Run(){
            _isStopped = false;
            this._waitHandle.Set();

            while (_isRunning){
                this._waitHandle.WaitOne();

                if (!_isRunning)
                    break;

                while (this._queue.TryDequeue(out P2PFile file)){
                    bool uploaded = true;

                    if (!_peers.Any(p => p.Value.IsOnline())){
                        this._queue.Enqueue(file);
                        break;
                    }

                    
                    
                    if (!_isRunning){
                        this._queue.Enqueue(file);
                        break;
                    }

                    string filePath = file.paths[0];
                    string compressedFilePath = this._path + @".hidden\" + file.hash;

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

                    List<string> chunks =
                        splitter.SplitFile(encryptedFilePath, file.hash, _path + @".hidden\splitter\");
                    file.AddChunk(chunks);

                    FileUploader uploader = new FileUploader(_ports, _peers);

                    int i = 0;
                    foreach (var chunk in file.chunks){
                        string path = _path + @".hidden\splitter\" + chunk.hash;

                        //3 is used, because the network is relatively small. 10 is the default.
                        if(!uploader.Push(chunk, path, 3, i)) {
                            uploaded = false;
                        }
                        i++;
                    }

                    if (!uploaded){
                        this._queue.Enqueue(file);
                    }

                    if (uploaded){
                        Console.WriteLine();
                        DiskHelper.ConsoleWrite($"The file {file.hash} was successfully sent to all \n");
                    }
                }

                this._waitHandle.Reset();
            }

            _isStopped = true;
        }

        /// <summary>
        /// This shutdowns the UploadManager.
        /// </summary>
        /// <returns>Returns true when success.</returns>
        public override bool Shutdown(){
            this._isRunning = false;
            this._waitHandle.Set();

            Console.Write("Upload thread stopping... ");
            while (!this._isStopped){ }

            Console.Write("Stopped!\n");

            return true;
        }
    }
}