﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace P2P_lib
{
    [Serializable]
    public class P2PFile
    {
        public readonly string Hash;
        public readonly List<string> Paths;
        public readonly List<P2PChunk> Chunks;

        public P2PFile(string hash){
            this.Paths = new List<string>();
            this.Chunks = new List<P2PChunk>();
            this.Hash = hash;
        }
        [JsonConstructor]        
        private P2PFile(string hash,List<P2PChunk> Chunks,List<string> paths){
            this.Paths = paths;
            this.Chunks = Chunks;
            this.Hash = hash;
        }


        public void AddPath(string path){
            this.Paths.Add(path);
        }

        public void AddPath(List<string> paths){
            foreach (string path in paths)
            {
                this.Paths.Add(path);
            }
        }

        public void AddChunk(P2PChunk chunk){
            this.Chunks.Add(chunk);
        }

        public void AddChunk(List<string> chunks){
            foreach(String chunk_hash in chunks){
                this.Chunks.Add(new P2PChunk(chunk_hash));
            }
        }

        public Boolean Downloaded(string path){
            foreach(P2PChunk chunk in this.Chunks){
                if(!chunk.exist(this.GetChunkDirectory(path))){
                    return false;
                }
            }
            return true;
        }

        public string GetChunkDirectory(string path){
            return path + @"\" + this.Hash;
        }
    }
}
