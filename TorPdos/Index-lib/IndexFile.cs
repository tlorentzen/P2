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
                addPath(path);
            }
        }

        public void addPath(string path, bool ghost=false) {
            if (!ghost && File.Exists(path)) {
                paths.Add(path);
                makeFileHash();
            }

            if(paths.Count == 1){
                size = new FileInfo(path).Length;
            }

            this.ghost = ghost;
        }

        // Deep copy.
        public IndexFile copy(){
            IndexFile file = new IndexFile();
            file.hash = hash;
            file.size = size;
            file.ghost = ghost;

            foreach (string path in paths){
                file.paths.Add(path);
            }

            return file;
        }

        public bool isGhostFile() {
            return ghost;
        }

        public string getHash() {
            return hash;
        }

        public string getPath(int pathNumber = 0) {
            int _pathNumber = (pathNumber > paths.Count ? paths.Count -1 : pathNumber);
            return paths[_pathNumber];
        }

        private void makeFileHash() {
            using (var md5 = MD5.Create())
            {
                using (FileStream fs = new FileStream(paths[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                    var hash = md5.ComputeHash(fs);
                    this.hash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    fs.Close();
                }
            }
        }

        public void rehash() {
            makeFileHash();
        }

        public override bool Equals(object obj){
            return (obj is IndexFile) && ((IndexFile)obj).hash == hash;
        }

    }
}
