using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace P2P_lib
{
    [Serializable]
    public class P2PFile{
        public readonly string hash;
        public readonly List<string> paths;
        public readonly List<P2PChunk> chunks;

        public P2PFile(string hash){
            this.hash = hash;
            this.paths = new List<string>();
            this.chunks = new List<P2PChunk>();
        }

        [JsonConstructor]        
        private P2PFile(string hash, List<P2PChunk> chunks, List<string> paths){
            this.hash = hash;
            this.paths = paths;
            this.chunks = chunks;
        }

        /// <summary>
        /// Adds one path to the file
        /// </summary>
        /// <param name="path">Path to be added to the file</param>
        public void AddPath(string path){
            this.paths.Add(path);
        }

        /// <summary>
        /// Adds a list of paths to the file
        /// </summary>
        /// <param name="pathList">List of paths to be added to the file</param>
        public void AddPath(List<string> pathList){
            foreach (string path in pathList){
                this.paths.Add(path);
            }
        }

        /// <summary>
        /// Adds a chunk to the file
        /// </summary>
        /// <param name="chunk">Chunk to be added to the file</param>
        public void AddChunk(P2PChunk chunk){
            chunk.originalHash = this.hash;
            this.chunks.Add(chunk);
        }

        /// <summary>
        /// Adds a list of chunks to the file
        /// </summary>
        /// <param name="chunkList">List of chunks to be added to the file</param>
        /// <returns>Rather the chunks where added</returns>
        public bool AddChunk(List<string> chunkList){
            foreach(String chunkHash in chunkList){
                this.chunks.Add(new P2PChunk(chunkHash, this.hash));
            }

            return true;
        }

        /// <summary>t
        /// Removes the chunk from the file
        /// </summary>
        /// <param name="chunkHash">Chunk to be added</param>
        /// <returns>Rather the chunk was removed</returns>
        public bool RemoveChunk(string chunkHash){
            P2PChunk correctChunk = null;
            foreach (P2PChunk currentChunkInListOfChunks in this.chunks){
                if (currentChunkInListOfChunks.hash.Equals(chunkHash)){
                    correctChunk = currentChunkInListOfChunks;
                    break;
                }
            }

            if(correctChunk != null){
                this.chunks.Remove(correctChunk);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks rather all the chunks of the file has been downloaded
        /// </summary>
        /// <param name="path">Path to the folder of the downloadedchunks</param>
        /// <returns>Rather all the chunks have been downloaded</returns>
        public bool Downloaded(string path){
            foreach(P2PChunk chunk in this.chunks){
                if(!chunk.Exist(this.GetChunkDirectory(path))){
                    return false;
                } 

                if (!CheckMD5Chunk(this.GetChunkDirectory(path)+"\\"+chunk.hash,chunk.hash)){
                    File.Delete(this.GetChunkDirectory(path) + "\\" + chunk.hash);
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
            foreach (var chunk in this.chunks){
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
            return path + this.hash;
        }
        
        /// <summary>
        /// Checks MD5 of chunk.
        /// </summary>
        /// <param name="filename">File path</param>
        /// <param name="inputHash">Hash of the chunk</param>
        /// <returns>True if they are equal</returns>
        private bool CheckMD5Chunk(string filename,string inputHash){
            using (var md5 = MD5.Create()){
                using (var stream = File.OpenRead(filename)){
                    var fileHash = md5.ComputeHash(stream);
                    return BitConverter.ToString(fileHash).Replace("-", "").Equals(inputHash);
                }
            }
        }
    }
}
