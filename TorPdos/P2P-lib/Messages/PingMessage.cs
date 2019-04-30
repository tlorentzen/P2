using System;

namespace P2P_lib.Messages
{
    [Serializable]
    public class PingMessage : BaseMessage
    {
        private long _pinged;
        public long diskSpace;

        public PingMessage(Peer to) : base(to)
        {
            _pinged = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public long GetPingTime()
        {
            return this._pinged;
        }

        public long GetElapsedTime()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() - _pinged;
        }

        public override string GetHash(){
            return null;
        }

    }
}
