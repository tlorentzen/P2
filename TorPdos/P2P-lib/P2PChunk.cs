using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace P2P_lib
{
    [Serializable]
    public class P2PChunk {
        public readonly string hash;
        public string originalHash;
        public readonly List<string> peers;

        public P2PChunk(string hash, string orgHash) {
            this.hash = hash;
            this.originalHash = orgHash;
            this.peers = new List<string>();
        }

        public P2PChunk(string chunkHash, string orgHash, List<string> peers){
            this.hash = chunkHash;
            this.originalHash = orgHash;
            this.peers = peers;
        }
        
        [JsonConstructor]
        private P2PChunk(string hash, List<string> peers,int fetchCount){
            this.hash = hash;
            this.peers = peers;
        }

        /// <summary>
        /// Adds a peer the list of receiving peers.
        /// </summary>
        /// <param name="peer">The UUID of the peer to add.</param>
        public void AddPeer(string peer){
            this.peers.Add(peer);
        }

        /// <summary>
        /// Removes a peer from the list of peers on the chunk.
        /// </summary>
        /// <param name="peer">The UUID of the peer to remove</param>
        /// <returns>Returns boolean on success</returns>
        public bool RemovePeer(string peer){
            this.peers.Remove(peer);
            return true;
        }

        /// <summary>
        /// Checks whether the chunk exists.
        /// </summary>
        /// <param name="path">Base path to where the chunk is placed.</param>
        /// <returns></returns>
        public bool Exist(string path){
            return File.Exists(path + @"\" + this.hash);
        }

        /// <summary>
        /// Gets the path of the file.
        /// </summary>
        /// <param name="basePath">The base path to where the chunks are stored.</param>
        /// <returns></returns>
        public string Path(string basePath){
            return basePath + @"\" + this.originalHash + this.hash;
        }
    }
}
