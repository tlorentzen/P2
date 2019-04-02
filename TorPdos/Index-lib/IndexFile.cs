using System;
using System.Collections.Generic;
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
                this.addPath(path);
            }
        }

        public void addPath(string path, bool ghost=false) {
            if (!ghost && File.Exists(path)) {
                this.paths.Add(path);
                this.makeFileHash();
            }

            if(this.paths.Count == 1){
                this.size = new FileInfo(path).Length;
            }

            this.ghost = ghost;
        }

        public bool isGhostFile() {
            return this.ghost;
        }

        public string getHash() {
            return this.hash;
        }

        public string getPath(int pathNumber = 0) {
            int _pathNumber = (pathNumber > paths.Count ? paths.Count -1 : pathNumber);
            return this.paths[_pathNumber];
        }

        private void makeFileHash() {
            using (var md5 = MD5.Create())
            {
                using (FileStream fs = new FileStream(this.paths[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                    var hash = md5.ComputeHash(fs);
                    this.hash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
        }

        public void rehash() {
            this.makeFileHash();
        }

        public override bool Equals(object obj){
            return (obj is IndexFile) && ((IndexFile)obj).hash == this.hash;
        }

    }
}
