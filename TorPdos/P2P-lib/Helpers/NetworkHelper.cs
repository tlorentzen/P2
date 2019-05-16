using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System;

namespace P2P_lib{
    public class NetworkHelper{
        public static string GetLocalIpAddress(){
            // https://stackoverflow.com/questions/6803073/get-local-ip-address
            string localIp;

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)){
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIp = endPoint.Address.ToString();
                socket.Close();
            }

            return localIp;
        }

        public static string GetPublicIpAddress(){
            // https://stackoverflow.com/questions/3253701/get-public-external-ip-address/45242105
            return new WebClient().DownloadString("http://icanhazip.com");
        }

        public static List<string> GetMacAddresses(){
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

        public static string MacAddressCombiner()
        {
            string guid = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            List<string> macAddresses = GetMacAddresses();

            foreach (string mac in macAddresses)
            {
                guid += mac;
            }

            return DiskHelper.CreateMd5(guid);
        }

    }
}