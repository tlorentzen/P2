using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace P2P_lib
{
    public class NetworkPorts
    {
        private List<int> ports = new List<int>();

        public int GetAvailablePort(int begin_range = 49152, int end_range = 65535)
        {
            for (int i = begin_range; i <= end_range; i++)
            {
                if (isPortAvailable(i) && !ports.Contains(i))
                {
                    ports.Add(i);
                    return i;
                }
            }

            return 0;
        }

        public void Release(int port){
            if(this.ports.Contains(port)){
                this.ports.Remove(port);
            }
        }

        public Boolean isPortAvailable(int port)
        {
            Boolean isAvailable = true;
            //https://stackoverflow.com/questions/570098/in-c-how-to-check-if-a-tcp-port-is-available
            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;
        }

    }
}
