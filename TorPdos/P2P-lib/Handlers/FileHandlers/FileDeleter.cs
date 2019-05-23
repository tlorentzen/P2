using System.Collections.Concurrent;
using System.Net;
using NLog;
using P2P_lib.Helpers;
using P2P_lib.Messages;
using TypeCode = P2P_lib.Messages.TypeCode;

namespace P2P_lib{
    public class FileDeleter{
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private NetworkPorts _ports;
        private readonly int _port;
        private readonly IPAddress _ip;
        private readonly byte[] _buffer = new byte[1024];
        private static readonly Logger Logger = LogManager.GetLogger("FileDeleter");

        public FileDeleter(ConcurrentDictionary<string, Peer> peers, NetworkPorts ports){
            _ports = ports;
            _peers = peers;
            _port = _ports.GetAvailablePort();
            _ip = IPAddress.Any;
        }

        /// <summary>
        /// Handles the deletion of a chunk of the file
        /// </summary>
        /// <param name="currentFileChunk">The P2PChunk to be deleted</param>
        /// <param name="currentFile">The P2PFile to wich the chunk belongs</param>
        /// <returns></returns>
        public bool ChunkDeleter(P2PChunk currentFileChunk, P2PFile currentFile){
            Listener listener = new Listener(this._port);

            //Sends a delete message to every peer with the chunk
            foreach (var receivingPeers in currentFileChunk.peers){
                if (_peers.TryGetValue(receivingPeers, out Peer currentReceiver)) {
                    var deletionMessage = new FileDeletionMessage(currentReceiver) {
                        type = TypeCode.REQUEST,
                        statusCode = StatusCode.OK,
                        port = _port,
                        fileHash = currentFileChunk.hash,
                        fullFileHash = currentFile.Hash
                    };

                    //Sends the message and waits for a response,
                    //which will then overwrite the original sent message
                    if (listener.SendAndAwaitResponse(ref deletionMessage, 2000)) {
                        if (deletionMessage.type.Equals(TypeCode.RESPONSE)) {
                            switch (deletionMessage.statusCode) {
                                case StatusCode.OK when currentFileChunk.peers.Count == 0:
                                    currentFile.RemoveChunk(currentFileChunk.hash);
                                    break;
                                case StatusCode.OK when currentFileChunk.peers.Count == 0:
                                    currentFile.RemoveChunk(currentFileChunk.hash);
                                    break;
                                case StatusCode.OK:
                                    currentFileChunk.RemovePeer(deletionMessage.fromUuid);
                                    break;
                                case StatusCode.FILE_NOT_FOUND: {
                                    if (currentFileChunk.peers.Count == 0) {
                                        currentFile.RemoveChunk(currentFileChunk.hash);
                                    } else {
                                        currentFileChunk.RemovePeer(deletionMessage.fromUuid);
                                    }

                                    DiskHelper.ConsoleWrite("File not found at peer");
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}