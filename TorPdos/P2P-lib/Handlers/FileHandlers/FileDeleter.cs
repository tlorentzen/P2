using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NLog;
using P2P_lib.Helpers;
using P2P_lib.Messages;
using TypeCode = P2P_lib.Messages.TypeCode;

namespace P2P_lib{
    public class FileDeleter{
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private NetworkPorts _ports;
        private readonly int _port;
        private TcpListener _server;
        private readonly IPAddress _ip;
        private readonly byte[] _buffer = new byte[1024];
        private static readonly Logger Logger = LogManager.GetLogger("FileDeleter");

        public FileDeleter(ConcurrentDictionary<string, Peer> peers, NetworkPorts ports){
            _ports = ports;
            _peers = peers;
            _port = _ports.GetAvailablePort();
            _ip = IPAddress.Any;
        }

        public bool ChunkDeleter(P2PChunk currentFileChunk, P2PFile currentFile){
            _server = new TcpListener(this._ip, this._port);
            try{
                _server.AllowNatTraversal(true);
                _server.Start();
            }
            catch (Exception e){
                Logger.Error(e);
            }



            foreach (var receivingPeers in currentFileChunk.peers){
                if (!_peers.TryGetValue(receivingPeers, out Peer currentReceiver)) continue;
                
                var deletionMessage = new FileDeletionMessage(currentReceiver){
                    type = TypeCode.REQUEST,
                    statusCode = StatusCode.OK,
                    port = _port,
                    fileHash = currentFileChunk.hash,
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
                    if (msg.GetMessageType() != typeof(FileDeletionMessage)) return false;
                    var message = (FileDeletionMessage) msg;

                    if (!message.type.Equals((TypeCode.RESPONSE))) return false;
                    switch (message.statusCode){
                        case StatusCode.OK when currentFileChunk.peers.Count == 0:
                            currentFile.RemoveChunk(currentFileChunk.hash);
                            break;
                        case StatusCode.OK when currentFileChunk.peers.Count == 0:
                            currentFile.RemoveChunk(currentFileChunk.hash);
                            break;
                        case StatusCode.OK:
                            currentFileChunk.RemovePeer(message.fromUuid);
                            break;
                        case StatusCode.FILE_NOT_FOUND:{
                            if (currentFileChunk.peers.Count == 0){
                                currentFile.RemoveChunk(currentFileChunk.hash);
                            } else{
                                currentFileChunk.RemovePeer(message.fromUuid);
                            }

                            DiskHelper.ConsoleWrite("File not found at peer");
                            _server.Stop();
                            return false;
                        }
                    }
                }
            }

            _server.Stop();

            return true;
        }
    }
}