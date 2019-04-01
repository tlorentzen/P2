using System.Collections.Generic;
using static P2P_lib.Network;

namespace P2P_lib.Messages{
    public class PeerFetcherMessage : BaseMessage{
        public PeerFetcherMessage(string to) : base(to){ }

        public override string GetHash(){
            return null;
        }

        public List<Peer> Peers;

        
        /// <summary>
        /// This is the response function to a request of a list of peers.
        /// </summary>
        /// <param name="input"></param>
        public void sendPeers(List<Peer> input){
            this.type = TypeCode.RESPONSE;
            this.statuscode = StatusCode.OK;
            string from_Ip = this.@from;
            this.to = from_Ip;
            Peers = input;
        }
        
    }
}