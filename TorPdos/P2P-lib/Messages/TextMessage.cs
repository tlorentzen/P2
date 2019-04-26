using System;

namespace P2P_lib.Messages
{
    [Serializable]
    public class TextMessage : BaseMessage
    {
        private string _message;

        public TextMessage(Peer to) : base(to)
        {

        }

        public void SetMessage(string msg)
        {
            this._message = msg;
        }

        public string GetMessage()
        {
            return this._message;
        }

        public override string GetHash()
        {
            return null;
        }
    }
}
