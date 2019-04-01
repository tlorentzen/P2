using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.IO;
using System.Net;
using System.Net.Sockets;
using P2P_lib.Messages;
using Index_lib;
using P2P_lib;
using TorPdos;

namespace P2P_lib {
    public class NetworkProtocols {
        public int UploadFileToNetwork (string filePath, int seed, Network network) {
            List<Peer> peerlist = network.getPeerList();
            for (int i = 0; i < seed; i++) {
                
            }
            return seed;
        }
    }
}
