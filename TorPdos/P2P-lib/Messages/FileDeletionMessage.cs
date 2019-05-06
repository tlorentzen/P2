using System;

namespace P2P_lib.Messages{
    [Serializable]
    public class FileDeletionMessage : BaseMessage{
        public string fileHash;
        public string fullFileHash;
        public long filesize;
        public string path;
        public int port;
        public FileDeletionMessage(Peer to) : base(to){
            
        }
        public override string GetHash(){
            return null;
        }
    }
}