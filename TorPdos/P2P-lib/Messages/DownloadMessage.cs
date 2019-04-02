﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P_lib.Messages
{
    [Serializable]
    public class DownloadMessage : BaseMessage
    {
        public string filehash;
        public string filename;
        public string path;
        public long filesize;
        public int port;

        public DownloadMessage(String to) : base(to)
        {

        }

        public override String GetHash()
        {
            return null;
        }
    }
}