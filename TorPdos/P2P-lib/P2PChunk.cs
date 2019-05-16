using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace P2P_lib
{
    [Serializable]
    public class P2PChunk
    {
        public readonly string Hash;
        public string OriginalHash;
        public readonly List<string> Peers;
        public int fetch_count = 0;
        
        public P2PChunk(string hash, string org_hash)
        {
            this.Hash = hash;
            this.OriginalHash = org_hash;
            this.Peers = new List<string>();
        }

        public P2PChunk(string chunk_hash, string org_hash, List<string> peers){
            this.Hash = chunk_hash;
            this.OriginalHash = org_hash;
            this.Peers = peers;
        }
        
        [JsonConstructor]
        private P2PChunk(string hash, List<string> peers,int fetchCount){
            Hash = hash;
            Peers = peers;
            fetch_count = fetchCount;
        }

        public void AddPeer(string peer){
            this.Peers.Add(peer);
        }

        public bool RemovePeer(string peer){
            this.Peers.Remove(peer);
            return true;
        }

        public bool exist(string path){
            return File.Exists(path + @"\" + this.Hash);
        }

        public string Path(string base_path){
            return base_path + @"\" + this.Hash;
        }
    }
}
