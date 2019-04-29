using System;
using System.IO;
using Newtonsoft.Json;

namespace P2P_lib{
    [Serializable]
    public class QueuedFile{
        [JsonProperty]
        private string _hash;
        [JsonProperty]
        private string _path;
        public int _copies = 5;
        [JsonProperty]
        private long _filesize = 0;
        [JsonProperty]
        private string _filename;
        [JsonProperty]
        private bool _ghost;
        [JsonProperty]
        private int _port;
        [JsonProperty]
        private Peer peer;

        public QueuedFile(string hash) : this(hash, null, 0){
            this._ghost = false;
        }
        [JsonConstructor]
        private QueuedFile(string hash, string path, int copies,long filesize, string filename, bool ghost, int port, Peer peer){
            _hash = hash;
            _path = path;
            _copies = copies;
            _filesize = filesize;
            _filename = filename;
            _port = port;
            this.peer = peer;
        }

        public QueuedFile(string hash, string path, int copies){
            this._hash = hash;
            this._copies = (copies <= 0 ? _copies : copies);

            if (path != null){
                this._path = path;
                this._filesize = new System.IO.FileInfo(this._path).Length;
                this._filename = new FileInfo(this._path).Name;
            }

            this._ghost = true;
        }

        public string GetHash(){
            return this._hash;
        }

        public string GetPath(){
            return this._path;
        }

        public int GetCopies(){
            return this._copies;
        }

        public long GetFilesize(){
            return this._filesize;
        }

        public void SetFilesize(long filesize){
            this._filesize = filesize;
        }

        public string GetFilename(){
            return this._filename;
        }

        public void SetFilename(string filename){
            this._filename = filename;
        }

        public bool IsGhostFile(){
            return this._ghost;
        }
    }
}