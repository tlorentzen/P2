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
                AddPath(path);
            }
        }

        public void AddPath(string path, bool ghost=false) {
            if (!ghost && File.Exists(path)) {
                paths.Add(path);
                MakeFileHash();
            }

            if(paths.Count == 1){
                size = new FileInfo(path).Length;
            }

            this.ghost = ghost;
        }

        // Deep copy.
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

        public string GetPath(int pathNumberInput = 0) {
            int pathNumber = (pathNumberInput > paths.Count ? paths.Count -1 : pathNumberInput);
            return paths[pathNumber];
        }

        private void MakeFileHash() {
            using (var md5 = MD5.Create())
            {
                using (FileStream fs = new FileStream(paths[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                    var hash = md5.ComputeHash(fs);
                    this.hash = BitConverter.ToString(hash).Replace("-", "").ToLower();
                    fs.Close();
                }
            }
        }

        public void Rehash() {
            MakeFileHash();
        }

        public override bool Equals(object obj){
            return (obj is IndexFile) && ((IndexFile)obj).hash == hash;
        }

    }
}
