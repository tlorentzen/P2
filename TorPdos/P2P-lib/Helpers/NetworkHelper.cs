using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace P2P_lib.Helpers{
    public static class NetworkHelper{
        /// <summary>
        /// Gets the local IP-address.
        /// </summary>
        /// <returns>The local IP-address as a string</returns>
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

        /// <summary>
        /// Gets the public IP-address.
        /// </summary>
        /// <returns>The public IP-address as a string.</returns>
        public static string GetPublicIpAddress(){
            // https://stackoverflow.com/questions/3253701/get-public-external-ip-address/45242105
            return new WebClient().DownloadString("http://icanhazip.com");
        }

        /// <summary>
        /// Gets the Mac-addresses.
        /// </summary>
        /// <returns>The Mac-addresses as a list of strings.</returns>
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

        /// <summary>
        /// Combines every Mac-address and the current time into a string.
        /// Then hashes the string.
        /// </summary>
        /// <returns>The hash of the current time and combined Mac-addresses.</returns>
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