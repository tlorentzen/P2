using System.Collections.Concurrent;
using System.Net;
using P2P_lib.Helpers;
using P2P_lib.Messages;
using TypeCode = P2P_lib.Messages.TypeCode;

namespace P2P_lib.Handlers.FileHandlers{
    public class FileDeleter{
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly int _port;

        public FileDeleter(ConcurrentDictionary<string, Peer> peers, NetworkPorts ports){
            _peers = peers;
            _port = ports.GetAvailablePort();
        }

        /// <summary>
        /// Handles the deletion of a chunk of the file
        /// </summary>
        /// <param name="currentFileChunk">The P2PChunk to be deleted</param>
        /// <param name="currentFile">The P2PFile to wich the chunk belongs</param>
        /// <returns></returns>
        public bool ChunkDeleter(P2PChunk currentFileChunk, P2PFile currentFile){
            Listener listener = new Listener(this._port);

            int lastIndex = currentFileChunk.peers.Count - 1;
            //Sends a delete message to every peer with the chunk
            for (int i = lastIndex; i >= 0; i--) {
                if (_peers.TryGetValue(currentFileChunk.peers[i], out Peer currentReceiver)) {
                    if (!currentReceiver.IsOnline()){
                        return false;
                    }
                    var deletionMessage = new FileDeletionMessage(currentReceiver) {
                        type = TypeCode.REQUEST,
                        statusCode = StatusCode.OK,
                        port = _port,
                        fileHash = currentFileChunk.hash,
                        fullFileHash = currentFile.hash
                    };

                    //Sends the message and waits for a response,
                    //which will then overwrite the original sent message
                    if (listener.SendAndAwaitResponse(ref deletionMessage, 2000)) {
                        if (deletionMessage.type.Equals(TypeCode.RESPONSE)) {
                            currentFileChunk.RemovePeer(deletionMessage.fromUuid);
                            if (currentFileChunk.peers.Count == 0) {
                                currentFile.RemoveChunk(currentFileChunk.hash);
                            }
                            if (deletionMessage.statusCode.Equals(StatusCode.FILE_NOT_FOUND)) {
                                DiskHelper.ConsoleWrite("File not found at peer");
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}