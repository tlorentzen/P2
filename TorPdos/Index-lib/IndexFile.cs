using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;

namespace Index_lib
{
    [Serializable]
    public class IndexFile
    {
        public string hash;
        public long size;
        public bool ghost;

        public List<string> paths = new List<string>();

        public IndexFile():this(null) { }

        public IndexFile(string path) {
            if (path != null) {
                AddPath(path);
            }
        }

        /// <summary>
        /// Adds a path to a file already in index
        /// </summary>
        /// <param name="path">New path pointing to file already in index</param>
        /// <param name="ghostFile">Rather the file should be ignored, when downloading files</param>
        public void AddPath(string path, bool ghostFile=false) {
            if (!ghostFile && File.Exists(path)) {
                paths.Add(path);
                MakeFileHash();
            }

            if(paths.Count == 1){
                size = new FileInfo(path).Length;
            }

            this.ghost = ghostFile;
        }

        /// <summary>
        /// Makes a deep copy of self
        /// </summary>
        /// <returns>A deep copy IndexFile of self</returns>
        public IndexFile Copy(){
            IndexFile file = new IndexFile();
            file.hash = hash;
            file.size = size;
            file.ghost = ghost;

            foreach (string path in paths){
                file.paths.Add(path);
            }

            return file;
        }

        public bool IsGhostFile() {
            return ghost;
        }

        public string GetHash() {
            return hash;
        }

        /// <summary>
        /// Retrieves path to self. Standard -1,
        /// which takes the latest path of the IndexFile.
        /// Else it takes the specified entry
        /// </summary>
        /// <param name="pathNumberInput">Entry number of the path</param>
        /// <returns>One of the paths to the self</returns>
        public string GetPath(int pathNumberInput = -1) {
            int pathNumber = (pathNumberInput > paths.Count || pathNumberInput == -1 ? paths.Count -1 : pathNumberInput);
            return paths[pathNumber];
        }

        /// <summary>
        /// Makes hash for self, based on content
        /// </summary>
        private void MakeFileHash() {
            using (var md5 = MD5.Create())
            {
                using (FileStream fs = new FileStream(paths[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                    var fileHash = md5.ComputeHash(fs);
                    this.hash = BitConverter.ToString(fileHash).Replace("-", "").ToLower();
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// Makes a new hash based on content
        /// </summary>
        public void Rehash() {
            MakeFileHash();
        }

        public override bool Equals(object obj){
            return (obj is IndexFile) && ((IndexFile)obj).hash == hash;
        }

        protected bool Equals(IndexFile other){
            return string.Equals(hash, other.hash);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode(){
            return hash != null ? hash.GetHashCode() : 0;
        }
    }
}
