using System;
using System.Net.Sockets;
using System.Net;
using System.IO;


namespace P2P_lib{
    public class ChunkSender{
        IPAddress ip;
        private int port;
        const int ChunkSize = 1024;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("ChunkSender");

        public ChunkSender(string ip, int port){
            this.ip = IPAddress.Parse(ip);
            this.port = port;
        }

        public bool Send(string path){
            if (File.Exists(path)){
                try{
                    using (TcpClient client = new TcpClient(this.ip.ToString(), this.port)){
                        client.SendTimeout = 3000;
                        client.Client.SendTimeout = 3000;
                        using (NetworkStream stream = client.GetStream()){
                            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read,
                                FileShare.ReadWrite)){
                                int bytesRead;
                                byte[] buffer = new byte[ChunkSize];
                                while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0){
                                    stream.Write(buffer, 0, bytesRead < buffer.Length ? bytesRead : buffer.Length);
                                }

                                file.Close();
                            }

                            stream.Close();
                        }

                        client.Close();
                    }

                    return true;
                }
                catch (Exception e){
                    logger.Error(e);
                    return false;
                }
            } else{
                logger.Error(new FileNotFoundException());
                return false;
            }
        }
    }
}