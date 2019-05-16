using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;
using NLog.Targets;
using P2P_lib.Messages;
using Splitter_lib;
using TypeCode = P2P_lib.Messages.TypeCode;

namespace P2P_lib.Managers{
    public class DeletionManager : Manager{
        private bool _isRunning = true;
        private int _port;
        private TcpListener _server;
        private Thread _listener;
        private readonly NetworkPorts _ports;
        private readonly ManualResetEvent _waitHandle;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly StateSaveConcurrentQueue<string> _queue;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("DeletionManager");
        private ConcurrentDictionary<string, P2PFile> _filesList = new ConcurrentDictionary<string, P2PFile>();
        public bool isStopped;
        private IPAddress _ip;
        private readonly byte[] _buffer = new byte[1024];

        public DeletionManager(StateSaveConcurrentQueue<string> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, ConcurrentDictionary<string, P2PFile> locations){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
            this._waitHandle = new ManualResetEvent(false);
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            this._filesList = locations;
            _ip = IPAddress.Any;
            Peer.PeerSwitchedOnline += PeerWentOnline;
        }

        private void QueueElementAddedToQueue(){
            this._waitHandle.Set();
        }

        private void PeerWentOnline(){
            this._waitHandle.Set();
        }

        public void Run(){
            isStopped = false;
            while (_isRunning){
                if (!_isRunning){
                    break;
                }

                this._waitHandle.WaitOne();

                while (this._queue.TryDequeue(out var item)){
                    if (!_isRunning){
                        _waitHandle.Set();
                        break;
                    }
                    
                    _port = _ports.GetAvailablePort();

                    try{
                        _server = new TcpListener(this._ip, this._port);
                        _server.AllowNatTraversal(true);
                        _server.Start();
                    }
                    catch (Exception e){
                        Logger.Error(e);
                    }
                    
                    _filesList.TryGetValue(item, out var currentFile);
                    if (currentFile == null){
                        return;
                    }
                    
                    foreach (var currentFileChunk in currentFile.Chunks){
                        
                        foreach (var receivingPeers in currentFileChunk.Peers){
                            
                            if (!_peers.TryGetValue(receivingPeers, out Peer currentReceiver)) continue;

                            var deletionMessage = new FileDeletionMessage(currentReceiver){
                                type = TypeCode.REQUEST,
                                statusCode = StatusCode.OK,
                                port = _port,
                                fileHash = currentFileChunk.Hash,
                                fullFileHash = currentFile.Hash
                            };
                            deletionMessage.Send();
                        }
                        
                        var client = _server.AcceptTcpClient();
                        client.ReceiveTimeout = 5000;

                        using (NetworkStream stream = client.GetStream()){
                            using (var memory = new MemoryStream()){
                                int i;
                                
                                while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                                    memory.Write(_buffer, 0, Math.Min(i, _buffer.Length));
                                }

                                memory.Seek(0, SeekOrigin.Begin);
                                var messageBytes = new byte[memory.Length];
                                memory.Read(messageBytes, 0, messageBytes.Length);
                                memory.Close();

                                var msg = BaseMessage.FromByteArray(messageBytes);
                                if (msg.GetMessageType() != typeof(FileDeletionMessage)) continue;
                                var message = (FileDeletionMessage) msg;

                                if (!message.type.Equals((TypeCode.RESPONSE))) continue;
                                switch (message.statusCode){
                                    case StatusCode.OK when currentFileChunk.Peers.Count == 0:
                                        currentFile.RemoveChunk(currentFileChunk.Hash);
                                        break;
                                    case StatusCode.OK:
                                        currentFileChunk.RemovePeer(message.fromUuid);
                                        break;
                                    case StatusCode.FILE_NOT_FOUND:{
                                        if (currentFileChunk.Peers.Count == 0){
                                            currentFile.RemoveChunk(currentFileChunk.Hash);
                                        } else{
                                            currentFileChunk.RemovePeer(message.fromUuid);
                                        }

                                        DiskHelper.ConsoleWrite("File not found at peer");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                _waitHandle.Reset();
            }

            isStopped = true;
        }


        public override bool Shutdown(){
            _isRunning = false;
            _waitHandle.Set();
            Console.Write("Deletion thread stopping... ");
            while (!this.isStopped){ }

            Console.Write("Stopped!");
            return true;
        }
    }
}