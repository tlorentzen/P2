using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using P2P_lib.Messages;
using System.Linq;
using P2P_lib.Helpers;

namespace P2P_lib
{
    class FileUploader {
        private int _port;
        private string _path;
        private readonly IPAddress _ip;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("FileUploader");
        private readonly byte[] _buffer;
        private NetworkPorts _ports;
        private ConcurrentDictionary<string, Peer> _peers;

        public FileUploader(NetworkPorts ports, ConcurrentDictionary<string, Peer> peers, int bufferSize = 1024) {
            this._ports = ports;
            this._ip = IPAddress.Any;
            this._path = DiskHelper.GetRegistryValue("Path");
            this._buffer = new byte[bufferSize];
            this._peers = peers;
        }

        public bool Push(P2PChunk chunk, string chunk_path, int numberOfRecevingPeers = 10, int receiverOffset = 0) {
            this._port = _ports.GetAvailablePort();
            List<Peer> peers = this.GetPeers(numberOfRecevingPeers);
            FileInfo fileInfo = new FileInfo(chunk_path);
            Listener listener = new Listener(this._port);
            bool sendToAll = true;
            int listLength = peers.Count;
            int peerCount = 0;

            for(peerCount = 0; peerCount < numberOfRecevingPeers; peerCount++){
                Peer currentPeer = peers[(peerCount + receiverOffset) % listLength];
                var upload = new UploadMessage(currentPeer){
                    filesize = fileInfo.Length,
                    fullFilename = chunk.originalHash,
                    chunkHash = chunk.hash,
                    path = chunk_path,
                    port = this._port
                };

                if(listener.SendAndAwaitResponse(ref upload, 2000)) {
                    if(upload.statusCode == StatusCode.ACCEPTED){
                        ChunkSender sender = new ChunkSender(currentPeer.StringIp, upload.port);

                        if(sender.Send(chunk_path)){
                            DiskHelper.ConsoleWrite($"The chunk {chunk.hash} was sent to {currentPeer.GetUuid()}");
                            chunk.AddPeer(currentPeer.GetUuid());
                        }else{
                            sendToAll = false;
                        }
                        _ports.Release(upload.port);
                    }
                }

                _ports.Release(this._port);
            }

            return sendToAll;
        }

        private List<Peer> GetPeers(int count) {
            List<Peer> topPeers = _peers.Values.Where(peer => peer.IsOnline() == true).ToList<Peer>();
            topPeers.Sort(new ComparePeersByRating());
            if (topPeers.Count > 0) {
                int wantedLengthOfTopList = Math.Min(topPeers.Count, count);
                topPeers.RemoveRange(wantedLengthOfTopList, Math.Max(0, topPeers.Count - wantedLengthOfTopList));
            }

            return topPeers;
        }
    }
}
