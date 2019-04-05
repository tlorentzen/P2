using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace P2P_lib{
    public class FileSender{
        IPAddress ip;
        private int port;
        const int chunkSize = 1024;

        public FileSender(string ip, int port){
            this.ip = IPAddress.Parse(ip);
            this.port = port;
        }

        public void Send(string path){
            if (File.Exists(path)){
                using (TcpClient client = new TcpClient(this.ip.ToString(), this.port)){
                    using (NetworkStream stream = client.GetStream()){

                        using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            int bytesRead;
                            byte[] buffer = new byte[chunkSize];
                            Console.WriteLine(buffer);
                            while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0){
                                stream.Write(buffer, 0, buffer.Length);
                            }
                        }
                    }
                }
            } else{
                throw new NotImplementedException();
            }
        }
    }
}