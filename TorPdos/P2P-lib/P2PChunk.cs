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
        public int fetch_count = 0;
        
        public P2PChunk(string hash, string org_hash) {
            this.hash = hash;
            this.originalHash = org_hash;
            this.peers = new List<string>();
        }

        public P2PChunk(string chunk_hash, string org_hash, List<string> peers){
            this.hash = chunk_hash;
            this.originalHash = org_hash;
            this.peers = peers;
        }
        
        [JsonConstructor]
        private P2PChunk(string hash, List<string> peers,int fetchCount){
            this.hash = hash;
            this.peers = peers;
            fetch_count = fetchCount;
        }

        public void AddPeer(string peer){
            this.peers.Add(peer);
        }

        public bool RemovePeer(string peer){
            this.peers.Remove(peer);
            return true;
        }

        public bool exist(string path){
            return File.Exists(path + @"\" + this.hash);
        }

        public string Path(string base_path){
            return base_path + @"\" + this.originalHash + this.hash;
        }
    }
}
