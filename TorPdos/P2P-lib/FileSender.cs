using System;
using System.Net.Sockets;
using System.Net;
using System.IO;


namespace P2P_lib{
    public class FileSender{
        IPAddress ip;
        private int port;
        const int ChunkSize = 1024;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("FileSender");

        public FileSender(string ip, int port){
            this.ip = IPAddress.Parse(ip);
            this.port = port;
        }

        public void Send(string path){
            if (File.Exists(path)){
                try{
                    using (TcpClient client = new TcpClient(this.ip.ToString(), this.port)){
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
                }
                catch (Exception e){
                    logger.Error(e);
                }
            } else{
                logger.Error(new FileNotFoundException());
            }
        }
    }
}