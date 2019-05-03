using System;
using System.Collections.Generic;

namespace P2P_lib.Messages{

    [Serializable]
    public class PeerFetcherMessage : BaseMessage {
        public PeerFetcherMessage(Peer to) : base(to){ }

        public override string GetHash(){
            return null;
        }

        public List<Peer> peers;

        /// <summary>
        /// This is the response function to a request of a list of peers.
        /// </summary>
        /// <param name="input"></param>
        public void SendPeers(List<Peer> input){
            this.type = TypeCode.RESPONSE;
            this.statusCode = StatusCode.OK;
            string fromIp = this.from;
            this.to = fromIp;
            peers = input;
        }
        
    }
}