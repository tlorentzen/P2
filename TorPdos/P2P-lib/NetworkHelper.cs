using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace P2P_lib{
    public class NetworkHelper{
        public static String getLocalIPAddress(){
            // https://stackoverflow.com/questions/6803073/get-local-ip-address
            string localIP;

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)){
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }

            return localIP;
        }

        public static String getPublicIpAddress(){
            // https://stackoverflow.com/questions/3253701/get-public-external-ip-address/45242105
            return new WebClient().DownloadString("http://icanhazip.com");
        }

        public static List<String> getMacAddresses(){
            List<string> macAddresses = new List<string>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()){
                if (nic.NetworkInterfaceType.Equals(NetworkInterfaceType.Ethernet) ||
                    nic.NetworkInterfaceType.Equals(NetworkInterfaceType.Wireless80211) ||
                    nic.NetworkInterfaceType.Equals(NetworkInterfaceType.GigabitEthernet)){
                    macAddresses.Add(nic.GetPhysicalAddress().ToString());
                }
            }

            return macAddresses;
        }
    }
}