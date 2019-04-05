using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P_lib.Messages {
    public class TransferStatusMessage : BaseMessage {
        public string filehash;
        public TransferStatusMessage(Peer to) : base(to)
        {
            this.to = to.GetIP();
        }

        public override string GetHash() {
            return null;
        }
    }
}
