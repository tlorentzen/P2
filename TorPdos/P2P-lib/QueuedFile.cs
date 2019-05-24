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
        public readonly int copies = 5;
        [JsonProperty]
        private long _fileSize;
        [JsonProperty]
        private string _filename;
        [JsonProperty]
        private bool _ghost;
        [JsonProperty]
        private int _port;
        [JsonProperty]
        private Peer _peer;

        public QueuedFile(string hash) : this(hash, null, 0){
            this._ghost = false;
        }

        [JsonConstructor]
        private QueuedFile(string hash, string path, int copies,long fileSize, string filename, bool ghost, int port, Peer peer){
            _hash = hash;
            _path = path;
            this.copies = copies;
            _fileSize = fileSize;
            _filename = filename;
            _port = port;
            this._peer = peer;
        }

        /// <summary>
        /// Constructor of QueuedFile, which only takes hash, path
        /// and number of copies of the file, and sets the size and
        /// name based on actual fileinformation.
        /// </summary>
        /// <param name="hash">Hash of the file</param>
        /// <param name="path">Path of the file</param>
        /// <param name="copies">Number of copies of the file</param>
        public QueuedFile(string hash, string path, int copies){
            this._hash = hash;
            this.copies = (copies <= 0 ? this.copies : copies);

            if (path != null){
                this._path = path;
                this._fileSize = new FileInfo(this._path).Length;
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
            return this.copies;
        }
        public long GetFilesize(){
            return this._fileSize;
        }
        public void SetFilesize(long filesize){
            this._fileSize = filesize;
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