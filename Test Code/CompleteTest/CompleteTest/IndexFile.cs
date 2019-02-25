using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace CompleteTest
{
    class IndexFile
    {
        private String _hash;
        private Boolean _ghost;

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

            this._ghost = ghost;
        }

        public Boolean isGhostFile() {
            return this._ghost;
        }

        public String getHash() {
            return this._hash;
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
                    this._hash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
        }

        public void rehash() {
            this.makeFileHash();
        }

        public override bool Equals(Object obj){
            return (obj is IndexFile) && ((IndexFile)obj)._hash == this._hash;
        }

    }
}
