﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using P2P_lib.Helpers;
using P2P_lib.Messages;

namespace P2P_lib.Handlers.FileHandlers
{
    class FileUploader {
        private int _port;
        private NetworkPorts _ports;
        private ConcurrentDictionary<string, Peer> _peers;

        public FileUploader(NetworkPorts ports, ConcurrentDictionary<string, Peer> peers) {
            this._ports = ports;
            DiskHelper.GetRegistryValue("Path");
            this._peers = peers;
        }

        /// <summary>
        /// Pushes the inputted chunk to the network, sending it the required amount of peers.
        /// </summary>
        /// <param name="chunk">The chunk to push to the network</param>
        /// <param name="chunkPath">String path to the chunks</param>
        /// <param name="numberOfRecevingPeers">The number of peers to send the file to,
        /// this will be the amount of receivers, unless the network is smaller than the given input.</param>
        /// <param name="receiverOffset">The offset for whom to send the files to, this determines the spacing of the chunks on the peerlist.</param>
        /// <returns>Boolean of whether the push was a success.</returns>
        public bool Push(P2PChunk chunk, string chunkPath, int numberOfRecevingPeers = 10, int receiverOffset = 0) {
            this._port = _ports.GetAvailablePort();
            List<Peer> peers = this.GetPeers();
            FileInfo fileInfo = new FileInfo(chunkPath);
            Listener listener = new Listener(this._port);
            bool sendToAll = true;
            int listLength = peers.Count;
            int peerCount;
            int numberOfReceivers = Math.Min(numberOfRecevingPeers, listLength);

            for (peerCount = 0; peerCount < numberOfReceivers; peerCount++){
                Peer currentPeer = peers[(peerCount + receiverOffset) % listLength];
                var upload = new UploadMessage(currentPeer){
                    filesize = fileInfo.Length,
                    fullFilename = chunk.originalHash,
                    chunkHash = chunk.hash,
                    path = chunkPath,
                    port = this._port
                };

                if(listener.SendAndAwaitResponse(ref upload, 2000)) {
                    if(upload.statusCode == StatusCode.ACCEPTED){
                        ChunkSender sender = new ChunkSender(currentPeer.StringIp, upload.port);

                        if(sender.Send(chunkPath)){
                            DiskHelper.ConsoleWrite($"The chunk {chunk.hash} was sent to {currentPeer.GetUuid()}");
                            chunk.AddPeer(currentPeer.GetUuid());
                        }else{
                            sendToAll = false;
                        }
                        _ports.Release(upload.port);
                    }
                }
            }

            _ports.Release(this._port);
            return sendToAll;
        }

        /// <summary>
        /// Get a list of peers to send to, helper function to push.
        /// </summary>
        /// <param name="count">The amount of peers to get.</param>
        /// <returns>Return the peers sorted by ranking.</returns>
        private List<Peer> GetPeers(int count = 100) {
            List<Peer> topPeers = _peers.Values.Where(peer => peer.IsOnline()).ToList();
            topPeers.Sort(new ComparePeersByRating());
            if (topPeers.Count > 0) {
                int wantedLengthOfTopList = Math.Min(topPeers.Count, count);
                topPeers.RemoveRange(wantedLengthOfTopList, Math.Max(0, topPeers.Count - wantedLengthOfTopList));
            }

            return topPeers;
        }
    }
}
