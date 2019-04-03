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

namespace P2P_lib{
    public class FileReceiver{
        private IPAddress _ip;
        private int _port;
        private TcpListener _server = null;
        private bool _listening = false;
        private Thread _listener;
        private byte[] _buffer;
        private string _filename;
        private bool _hidden;
        private string _outputPath = @"C:\\TorPdos\";

        public FileReceiver(string filename, bool hidden, int port, int bufferSize = 1024){
            this._filename = filename;
            this._ip = IPAddress.Any;
            this._port = port;
            this._buffer = new byte[bufferSize];
            this._hidden = hidden;
            if (hidden) {
                this._outputPath += ".hidden\";
            }
        }

        public void start(){
            _server = new TcpListener(this._ip, this._port);
            _server.AllowNatTraversal(true);
            _server.Start();

            _listening = true;
            _listener = new Thread(this.connectionHandler);
            _listener.Start();
        }

        public void stop(){
            this._listening = false;
            _server.Stop();
        }

        private void connectionHandler(){

            while (this._listening){
                TcpClient client = _server.AcceptTcpClient();

                using (NetworkStream stream = client.GetStream()){
                    /*if (!Directory.Exists("output")) {
                        Directory.CreateDirectory("output");
                    }*/

                    Console.WriteLine("Receiving file");
                    using (var fileStream = File.Open(this._outputPath + this._filename, FileMode.OpenOrCreate, FileAccess.Write)){
                        Console.WriteLine("Creating file: " + this._filename);
                        int i;
                        /*long initialFileSize = stream.Length;
                        long remainingFileSize = stream.Length;
                        double lastPersentage = 0;*/

                        /*Console.WriteLine(initialFileSize.ToString());*/

                        while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                            //fileStream.Write(buffer, 0, (i < buffer.Length) ? i : buffer.Length);
                            fileStream.Write(_buffer, 0, _buffer.Length);
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
            return this._port;
        }
    }
}
