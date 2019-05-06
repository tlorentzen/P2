using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace P2P_lib {
    public class NetworkPorts {
        private List<int> ports = new List<int>();

        public int GetAvailablePort(int beginRange = 50000, int endRange = 65535) {
            for (int i = beginRange; i <= endRange; i++) {
                if (IsPortAvailable(i) && !ports.Contains(i)) {
                    ports.Add(i);
                    return i;
                }
            }

            return 0;
        }

        public void Release(int port) {
            if (this.ports.Contains(port)) {
                this.ports.Remove(port);
            }
        }

        public static bool IsPortAvailable(int port) {
            bool isAvailable = true;
            // This is a hybrid of the two following links
            // https://stackoverflow.com/questions/570098/in-c-how-to-check-if-a-tcp-port-is-available
            // https://gist.github.com/jrusbatch/4211535
            // Evaluate current system tcp connections, TCP Listeners and UDP Listeners.
            // We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            IPEndPoint[] tcpListenerEndPoints = ipGlobalProperties.GetActiveTcpListeners();
            IPEndPoint[] udpListenerEndPoints = ipGlobalProperties.GetActiveUdpListeners();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray) {
                if (tcpi.LocalEndPoint.Port == port) {
                    isAvailable = false;
                    break;
                }
            }

            if (isAvailable) {
                foreach (IPEndPoint tcpEndPoint in tcpListenerEndPoints) {
                    if (tcpEndPoint.Port == port) {
                        isAvailable = false;
                        break;
                    }
                }
            }

            if (isAvailable) {
                foreach (IPEndPoint udpEndPoint in udpListenerEndPoints) {
                    if (udpEndPoint.Port == port) {
                        isAvailable = false;
                        break;
                    }
                }
            }

            return isAvailable;
        }
    }
}