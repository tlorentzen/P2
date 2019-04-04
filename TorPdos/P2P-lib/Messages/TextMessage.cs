using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P_lib.Messages
{
    [Serializable]
    public class TextMessage : BaseMessage
    {
        private string message;

        public TextMessage(string fromUUID, string toUUID) : base(fromUUID, toUUID)
        {

        }

        public void setMessage(string msg)
        {
            this.message = msg;
        }

        public string getMessage()
        {
            return this.message;
        }

        public override String GetHash()
        {
            return null;
        }
    }
}
