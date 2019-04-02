using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P_lib.Messages {
    public class TransferStatusMessage : BaseMessage {
        public string filehash;
        public TransferStatusMessage(string to) : base(to) {
            this.to = to;
        }

        public override string GetHash() {
            return null;
        }
    }
}
