using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P2P_lib.Messages;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace P2P_lib
{
    public class FileReceiver
    {
        private IPAddress ip;
        private int port;
        private TcpListener server = null;
        private Boolean listening = false;
        private Thread listener;
        private byte[] buffer;
        private String filename;

        public FileReceiver(String filename, int bufferSize = 1024)
        {
            this.filename = filename;
            this.ip = IPAddress.Any;
            this.port = NetworkHelper.getAvailablePort(55000, 56000);
            this.buffer = new byte[bufferSize];
        }

        public void start()
        {
            server = new TcpListener(this.ip, this.port);
            server.AllowNatTraversal(true);
            server.Start();

            listening = true;
            listener = new Thread(this.connectionHandler);
            listener.Start();
        }

        public void stop()
        {
            this.listening = false;
            server.Stop();
        }

        private void connectionHandler()
        {

            while (this.listening)
            {
                TcpClient client = server.AcceptTcpClient();

                using (NetworkStream stream = client.GetStream())
                {
                    if (!Directory.Exists("output"))
                    {
                        Directory.CreateDirectory("output");
                    }

                    Console.WriteLine("Receiving file");

                    using (var fileStream = File.Open(@"output/" + this.filename, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        Console.WriteLine("Creating file: " + this.filename);
                        int i;
                        /*long initialFileSize = stream.Length;
                        long remainingFileSize = stream.Length;
                        double lastPersentage = 0;*/

                        /*Console.WriteLine(initialFileSize.ToString());*/

                        while ((i = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            //fileStream.Write(buffer, 0, (i < buffer.Length) ? i : buffer.Length);
                            fileStream.Write(buffer, 0, buffer.Length);
                        }
                        /*
                        while (stream.DataAvailable) {

                            Console.WriteLine("Getting data");

                            //This is the received data.
                            long size = stream.Read(buffer, 0, buffer.Length);

                            fileStream.Write(buffer, 0, buffer.Length);
                            /*remainingFileSize -= size;
                            double percentage = Math.Floor(100 - (double)remainingFileSize / (double)initialFileSize * 100);
                            if (Math.Abs(lastPersentage - percentage) > 4 || percentage == 0) {
                                Console.WriteLine("({2:0}%) There is {0} bytes left to receive out of {1} bytes", remainingFileSize, initialFileSize, percentage);
                                lastPersentage = percentage;
                            }*/
                        /*
                    }
                */

                        Console.WriteLine("File done downloading");
                    }
                }

                client.Close();
                this.stop();
            }
        }

        public int getPort(){
            return this.port;
        }
        
    }
}
