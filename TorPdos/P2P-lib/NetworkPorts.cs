using System.Net;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace P2P_lib{
    public class NetworkPorts{
        private readonly List<int> _ports = new List<int>();
        private int _port;

        /// <summary>
        /// Finds a port not in use, within the specified range.
        /// </summary>
        /// <param name="beginRange">Start of portrange.</param>
        /// <param name="endRange">End of portrange.</param>
        /// <returns>A free port or 0, if non is available within the range.</returns>
        public int GetAvailablePort(int beginRange = 50000, int endRange = 65535){
            if (_port == 0){
                _port = beginRange;
            }
            _port++;
            
            for (int i = _port; i <= endRange; i++){
                if (!_ports.Contains(i) && IsPortAvailable(i)){
                    if(_port >= endRange){
                        _port = beginRange;
                    }
                    _ports.Add(i);
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// Releases the port, and makes it available for another process.
        /// </summary>
        /// <param name="port">The port to be released.</param>
        public void Release(int port){
            if (this._ports.Contains(port)){
                this._ports.Remove(port);
            }
        }

        /// <summary>
        /// Checks rather the specified port is in use by another process.
        /// </summary>
        /// <param name="port">The prot to be checked.</param>
        /// <returns>Rather the port is in use or not.</returns>
        public static bool IsPortAvailable(int port){
            bool isAvailable = true;
            // This solution is inspired by:
            // https://stackoverflow.com/questions/570098/in-c-how-to-check-if-a-tcp-port-is-available
            // https://gist.github.com/jrusbatch/4211535
            // Evaluate current system tcp connections, TCP Listeners and UDP Listeners.
            // We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            IPEndPoint[] tcpListenerEndPoints = ipGlobalProperties.GetActiveTcpListeners();
            IPEndPoint[] udpListenerEndPoints = ipGlobalProperties.GetActiveUdpListeners();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray){
                if (tcpi.LocalEndPoint.Port == port){
                    isAvailable = false;
                    break;
                }
            }
            if (isAvailable){
                foreach (IPEndPoint tcpEndPoint in tcpListenerEndPoints){
                    if (tcpEndPoint.Port == port){
                        isAvailable = false;
                        break;
                    }
                }
            }
            if (isAvailable){
                foreach (IPEndPoint udpEndPoint in udpListenerEndPoints){
                    if (udpEndPoint.Port == port){
                        isAvailable = false;
                        break;
                    }
                }
            }
            return isAvailable;
        }
    }
}