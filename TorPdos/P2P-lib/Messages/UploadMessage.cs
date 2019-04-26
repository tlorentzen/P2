using System;

namespace P2P_lib.Messages
{
    [Serializable]
    public class UploadMessage : BaseMessage
    {
        public string filehash;
        public string filename;
        public long filesize;
        public string path;
        public int port;

        public UploadMessage(Peer to) : base(to)
        {

        } 

        public override string GetHash()
        {
            return null;
        }

    }
}
