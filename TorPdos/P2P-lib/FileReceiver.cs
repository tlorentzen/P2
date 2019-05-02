using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace P2P_lib{
    public class FileReceiver{
        private IPAddress _ip;
        private string _path;
        private int _port;
        private TcpListener _server;
        private Thread _listener;
        private byte[] _buffer;
        private string _filename;
        private string _uuid;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("FileReceiver");
        public delegate void FileDownlaoded(string path);
        public event FileDownlaoded FileSuccefullyDownloaded;
        private Boolean _fileReceived;

        public FileReceiver(string path, string filename, int port, bool hidden, int bufferSize = 1024){
            this._ip = IPAddress.Any;
            this._buffer = new byte[bufferSize];
            this._filename = filename;
            this._port = port;
            this._path = path;

            if (!Directory.Exists(this._path)){
                Directory.CreateDirectory(this._path);
            }
        }

        public void Start(){
            try{             
                _server = new TcpListener(this._ip, this._port);
                _server.AllowNatTraversal(true);
                _server.Start();
            }
            catch (Exception e){
                logger.Error(e);
            }

            try{
                _listener = new Thread(this.ConnectionHandler);
                _listener.Start();
            }
            catch (Exception e){
                logger.Error(e);
            }
        }

        public void Stop(){
            _server.Stop();
        }

        private void ConnectionHandler(){
            string path = this._path + this._filename;

            try{
                TcpClient client = _server.AcceptTcpClient();

                using (NetworkStream stream = client.GetStream()){
                    Console.WriteLine(@"Receiving file");

                    using (var fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write)){
                        Console.WriteLine("Creating file: " + this._filename);
                        int i;

                        while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                            fileStream.Write(_buffer, 0, (i < _buffer.Length) ? i : _buffer.Length);
                        }

                        Console.WriteLine(@"File done downloading");
                        fileStream.Close();
                        _fileReceived = true;
                    }

                    stream.Close();
                }

                client.Close();
                if (_fileReceived){
                    FileSuccefullyDownloaded.Invoke(path);
                }
            }
            catch (InvalidOperationException e){
                logger.Fatal(e);
            }
            catch (Exception e){
                logger.Error(e);
            }
            finally{
                _server.Stop();
                Stop();
            }
        }

        public int GetPort(){
            return this._port;
        }
    }
}