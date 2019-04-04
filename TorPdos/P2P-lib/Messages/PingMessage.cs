using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P_lib.Messages
{
    [Serializable]
    public class PingMessage : BaseMessage
    {
        private long _pinged;

        public PingMessage(string fromUUID, string toUUID) : base(fromUUID, toUUID)
        {
            _pinged = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public long getPingTime()
        {
            return this._pinged;
        }

        public long getElapsedTime()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() - _pinged;
        }

        public override string GetHash(){
            return null;
        }

    }
}
