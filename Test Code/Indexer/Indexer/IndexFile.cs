using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Indexer
{
    [Serializable]
    class IndexFile
    {
        public String hash;
        public long size;
        public Boolean ghost;

        public List<String> paths = new List<string>();

        public IndexFile():this(null) { }

        public IndexFile(String path) {
            if (path != null) {
                this.addPath(path);
            }
        }

        public void addPath(String path, Boolean ghost=false) {
            if (!ghost && File.Exists(path)) {
                this.paths.Add(path);
                this.makeFileHash();
            }

            if(this.paths.Count == 1){
                this.size = new FileInfo(path).Length;
            }

            this.ghost = ghost;
        }

        public Boolean isGhostFile() {
            return this.ghost;
        }

        public String getHash() {
            return this.hash;
        }

        public String getPath() {
            return this.paths[0];
        }

        private void makeFileHash() {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(this.paths[0]))
                {
                    var hash = md5.ComputeHash(stream);
                    this.hash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
        }

        public void rehash() {
            this.makeFileHash();
        }

        public override bool Equals(Object obj){
            return (obj is IndexFile) && ((IndexFile)obj).hash == this.hash;
        }

    }
}
