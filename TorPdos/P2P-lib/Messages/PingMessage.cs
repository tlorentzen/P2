using System;

namespace P2P_lib.Messages
{
    [Serializable]
    public class PingMessage : BaseMessage
    {
        private long _pinged;
        public long diskSpace;

        /// <summary>
        /// Adds the assignment of _pinged on top of the BaseMessage constructor.
        /// </summary>
        /// <param name="to">The receiver of the message</param>
        public PingMessage(Peer to) : base(to)
        {
            _pinged = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public long GetPingTime()
        {
            return this._pinged;
        }

        /// <summary>
        /// Gets the time the ping took to transfer.
        /// </summary>
        /// <returns>The time in seconds</returns>
        public long GetElapsedTime()
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return now - _pinged;
        }

        public override string GetHash(){
            return null;
        }

    }
}
