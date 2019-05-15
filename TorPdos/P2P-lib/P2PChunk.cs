using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace P2P_lib
{
    public class P2PChunk
    {
        public readonly string Hash;
        public readonly List<Peer> Peers;
        public int fetch_count = 0;

        public P2PChunk(string hash){
            this.Hash = hash;
            this.Peers = new List<Peer>();
        }

        public P2PChunk(string hash, List<Peer> peers){
            this.Hash = hash;
            this.Peers = peers;
        }

        public void AddPeer(Peer peer){
            this.Peers.Add(peer);
        }

        public Boolean exist(string path){
            return File.Exists(path + @"\" + this.Hash);
        }
    }
}
