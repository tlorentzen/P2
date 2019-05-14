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
        private readonly byte[] _buffer;
        private readonly string _filename;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("FileReceiver");
        public delegate void FileDownloaded(string path);
        public event FileDownloaded FileSuccessfullyDownloaded;
        private Boolean _fileReceived;

        public FileReceiver(string path, string filename, int port, bool hidden = false, int bufferSize = 1024){
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

        private void Stop(){
            _server.Stop();
        }

        private void ConnectionHandler(){
            string path = this._path + this._filename;

            try{
                var client = _server.AcceptTcpClient();
                client.ReceiveTimeout = 5000;

                using (NetworkStream stream = client.GetStream()){
                    DiskHelper.ConsoleWrite(@"Receiving file");

                    using (var fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write)){
                        DiskHelper.ConsoleWrite("Creating file: " + this._filename);
                        int i;

                        while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                            fileStream.Write(_buffer, 0, (i < _buffer.Length) ? i : _buffer.Length);
                        }

                        DiskHelper.ConsoleWrite(@"File done downloading");
                        fileStream.Close();
                        _fileReceived = File.Exists(path);
                    }

                    stream.Close();
                }

                client.Close();

                if (_fileReceived){
                    FileSuccessfullyDownloaded?.Invoke(path);
                }
            }
            catch (InvalidOperationException e){
                logger.Fatal(e);
            }
            catch (Exception e){
                logger.Error(e);
            }
            finally{
                Stop();
            }
        }

        public int GetPort(){
            return this._port;
        }
    }
}