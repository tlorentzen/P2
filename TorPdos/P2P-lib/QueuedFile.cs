using System.IO;

namespace P2P_lib{
    public class QueuedFile{
        private string _hash;
        private string _path;
        private int _copies = 5;
        private long _filesize = 0;
        private string _filename;
        private bool _ghost;
        private int _port;
        private Peer peer;

        public QueuedFile(string hash) : this(hash, null, 0){
            this._ghost = false;
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