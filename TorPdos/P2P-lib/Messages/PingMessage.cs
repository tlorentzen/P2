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

        public PingMessage(String to) : base(to){
            _pinged = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public long getPingTime()
        {
            return this._pinged;
        }

        public Boolean reply(){
            this.type = TypeCode.RESPONSE;
            this.statuscode = StatusCode.OK;
            String from_ip = this.from;
            this.from = this.to;
            this.to = from_ip;
            return this.Send();
        }

        public long getElapsedTime()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() - _pinged;
        }

        public override String GetHash(){
            return null;
        }

    }
}
