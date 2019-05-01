using System;

namespace P2P_lib.Messages
{
    [Serializable]
    public class DownloadMessage : BaseMessage
    {
        public string filehash;
        public string fullFileName;
        public long filesize;
        public string path;
        public int port;

        public DownloadMessage(Peer to) : base(to){
            forwardCount = 8;
        }

        public override string GetHash()
        {
            return null;
        }
    }
}
