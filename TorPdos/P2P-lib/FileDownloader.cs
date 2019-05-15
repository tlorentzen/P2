using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Encryption;
using Newtonsoft.Json;
using P2P_lib.Messages;

namespace P2P_lib{
    [Serializable]
    public class FileDownloader{
        private FileReceiver _fileReceiver;
        private TcpListener _server;
        private Thread _listener;
        private string _hash;
        private readonly List<Peer> _peersToAsk;
        private readonly int _port;
        private readonly IPAddress _ip;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("FileDownloader");
        private bool _fileReceived;
        private readonly string _path;
        private readonly byte[] _buffer;
        private readonly Receiver _receiver;
        private NetworkPorts _ports;
        private string _fullFileName;


        public FileDownloader(string hash, List<Peer> peersToAsk, NetworkPorts ports, string fullFileName,
            int bufferSize = 1024){
            _hash = hash;
            _ports = ports;
            this._ip = IPAddress.Any;
            _fullFileName = fullFileName;
            _peersToAsk = peersToAsk;
            _port = _ports.GetAvailablePort();
            this._path = DiskHelper.GetRegistryValue("Path");
            this._buffer = new byte[bufferSize];
            Start();
        }

        private bool Start(){
            foreach (var Peer in _peersToAsk){
                if (!Peer.IsOnline()) break;

                var downloadMessage = new DownloadMessage(Peer){
                    port = this._port,
                    fullFileName = _fullFileName,
                    filehash = _hash
                };
                downloadMessage.Send();
                try{
                    _server = new TcpListener(this._ip, this._port);
                    _server.AllowNatTraversal(true);
                    _server.Start();
                }
                catch (Exception e){
                    Logger.Error(e);
                }

                var client = _server.AcceptTcpClient();
                client.ReceiveTimeout = 5000;

                using (NetworkStream stream = client.GetStream()){
                    int i;
                    using (MemoryStream memory = new MemoryStream()){
                        while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                            memory.Write(_buffer, 0, Math.Min(i, _buffer.Length));
                        }

                        memory.Seek(0, SeekOrigin.Begin);
                        byte[] messageBytes = new byte[memory.Length];
                        memory.Read(messageBytes, 0, messageBytes.Length);
                        memory.Close();

                        var msg = BaseMessage.FromByteArray(messageBytes);
                        if (msg.GetMessageType() != typeof(DownloadMessage)) continue;
                        var download = (DownloadMessage) msg;

                        if (!download.type.Equals(Messages.TypeCode.RESPONSE)) continue;
                        
                        if (download.statusCode == StatusCode.ACCEPTED){
                            download.CreateReply();
                            download.type = Messages.TypeCode.REQUEST;
                            download.statusCode = StatusCode.ACCEPTED;
                            download.port = _port;
                            DiskHelper.ConsoleWrite("FileReceiver opened");
                            download.Send();
                            _ports.Release(download.port);

                            using (var fileStream = File.Open(_path + _fullFileName + _hash,
                                FileMode.OpenOrCreate, FileAccess.Write)){
                                DiskHelper.ConsoleWrite("Creating file: " + this._hash);

                                while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                                    fileStream.Write(_buffer, 0, (i < _buffer.Length) ? i : _buffer.Length);
                                }

                                DiskHelper.ConsoleWrite(@"File done downloading");
                                fileStream.Close();
                            }
                        }

                        if (download.statusCode == StatusCode.FILE_NOT_FOUND){
                            Console.WriteLine("File not found at peer.");
                        }
                    }
                }
            }
            Stop();
            return _fileReceived = File.Exists(_path + _fullFileName + _hash);;
        }

        private void Stop(){
            _server.Stop();
        }

        public int GetPort(){
            return this._port;
        }
    }
}