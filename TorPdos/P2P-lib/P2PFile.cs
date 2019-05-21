using System;
using System.Collections.Generic;
using System.Linq;
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
            this.Hash = hash;
            this.Paths = new List<string>();
            this.Chunks = new List<P2PChunk>();
        }

        [JsonConstructor]        
        private P2PFile(string hash, List<P2PChunk> Chunks, List<string> paths){
            this.Hash = hash;
            this.Paths = paths;
            this.Chunks = Chunks;
        }

        /// <summary>
        /// Adds one path to the file
        /// </summary>
        /// <param name="path">Path to be added to the file</param>
        public void AddPath(string path){
            this.Paths.Add(path);
        }

        /// <summary>
        /// Adds a list of paths to the file
        /// </summary>
        /// <param name="paths">List of paths to be added to the file</param>
        public void AddPath(List<string> paths){
            foreach (string path in paths) {
                this.Paths.Add(path);
            }
        }

        /// <summary>
        /// Adds a chunk to the file
        /// </summary>
        /// <param name="chunk">Chunk to be added to the file</param>
        public void AddChunk(P2PChunk chunk){
            chunk.originalHash = this.Hash;
            this.Chunks.Add(chunk);
        }

        /// <summary>
        /// Adds a list of chunks to the file
        /// </summary>
        /// <param name="chunks">List of chunks to be added to the file</param>
        /// <returns>Rather the chunks where added</returns>
        public bool AddChunk(List<string> chunks){
            foreach(String chunk_hash in chunks){
                this.Chunks.Add(new P2PChunk(chunk_hash, this.Hash));
            }

            return true;
        }

        /// <summary>
        /// Removes the chunk from the file
        /// </summary>
        /// <param name="chunk">Chunk to be added</param>
        /// <returns>Rather the chunk was removed</returns>
        public bool RemoveChunk(string chunk){
            
            this.Chunks.Remove(Chunks.First(chunks => Hash.Equals(chunk)));
            
            return true;
        }

        /// <summary>
        /// Checks rather all the chunks of the file has been downloaded
        /// </summary>
        /// <param name="path">Path to the folder of the downloadedchunks</param>
        /// <returns>Rather all the chunks have been downloaded</returns>
        public bool Downloaded(string path){
            foreach(P2PChunk chunk in this.Chunks){
                if(!chunk.Exist(this.GetChunkDirectory(path))){
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the chunk hashes for all chunks
        /// </summary>
        /// <returns>A list consisting of all chnuk hashes of the file</returns>
        public List<string> GetChunksAsString(){
            List<string> output=new List<string>();
            foreach (var chunk in this.Chunks){
                output.Add(chunk.hash);
            }

            return output;
        }

        /// <summary>
        /// Gets the path to the folder with the downloaded chunks
        /// </summary>
        /// <param name="path">Path of the downloaded files</param>
        /// <returns>The path to the folder with downloaded chunks</returns>
        private string GetChunkDirectory(string path){
            return path + this.Hash;
        }
    }
}
