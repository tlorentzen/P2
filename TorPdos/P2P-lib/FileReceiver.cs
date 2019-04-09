using System;
using System.Configuration;
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
        private string _path;
        private int _port;
        private TcpListener _server = null;
        private bool _listening = false;
        private Thread _listener;
        private byte[] _buffer;
        private string _filename;
        private bool _hidden;
        private string UUID;

        public FileReceiver(string path, string filename, int port, bool hidden, int bufferSize = 1024){

            this._ip = IPAddress.Any;
            this._buffer = new byte[bufferSize];
            this._hidden = hidden;
            this._filename = filename;
            this._port = port;
            this._path = path;

            if (!Directory.Exists(this._path)){
                Directory.CreateDirectory(this._path);
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

            /*
            while (this._listening){
            */
                TcpClient client = _server.AcceptTcpClient();

                using (NetworkStream stream = client.GetStream()){

                    Console.WriteLine("Receiving file");
                    using (var fileStream = File.Open(this._path + this._filename, FileMode.OpenOrCreate, FileAccess.Write)){
                        Console.WriteLine("Creating file: " + this._filename);
                        int i;
                      
                        while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                            fileStream.Write(_buffer, 0, (i < _buffer.Length) ? i : _buffer.Length);
                            //fileStream.Write(_buffer, 0, _buffer.Length);
                        }

                        Console.WriteLine("File done downloading");
                    }
                }

                client.Close();
                /*
                this.stop();
            }
            */
        }

        public int getPort(){
            return this._port;
        }
    }
}
