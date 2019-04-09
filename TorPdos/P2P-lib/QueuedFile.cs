using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P_lib
{
    public class QueuedFile
    {
        private string _hash;
        private string _path;
        private int _copies = 5;

        public QueuedFile(string hash) : this(hash, null, 5){}

        public QueuedFile(string hash, string path, int copies){
            this._hash = hash;
            this._path = path;
            this._copies = copies;
        }

        public string GetHash(){
            return this._hash;
        }

        public string GetPath(){
            return this._path;
        }
    }
}
