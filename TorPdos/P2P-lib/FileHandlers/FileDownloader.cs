using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using P2P_lib.Messages;

namespace P2P_lib{
    [Serializable]
    public class FileDownloader{
        private FileReceiver _fileReceiver;
        private TcpListener _server;
        private Thread _listener;
        private string _hash;
        private List<string> _peersToAsk;
        private int _port;
        private readonly IPAddress _ip;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("FileDownloader");
        private bool _fileReceived;
        private readonly string _path;
        private readonly byte[] _buffer;
        private readonly Receiver _receiver;
        private NetworkPorts _ports;
        private ConcurrentDictionary<string, Peer> _peers;


        public FileDownloader(NetworkPorts ports,ConcurrentDictionary<string,Peer> peers,int bufferSize = 1024){
            _ports = ports;
            this._ip = IPAddress.Any;
            _port = _ports.GetAvailablePort();
            this._path = DiskHelper.GetRegistryValue("Path")+@".hidden\incoming\";
            this._buffer = new byte[bufferSize];
            this._peers = peers;
        }

        public bool Fetch(P2PChunk chunk, string fullFileName){
            _hash = chunk.Hash;
            _peersToAsk = chunk.Peers;
            foreach (var Peer in _peersToAsk){
                _peers.TryGetValue(Peer, out var currentPeer);
                if (!currentPeer.IsOnline()) continue;
                var downloadMessage = new DownloadMessage(currentPeer){
                    port = this._port,
                    fullFileName = fullFileName,
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
                            int receiverPort = _ports.GetAvailablePort();
                            download.CreateReply();
                            download.type = Messages.TypeCode.REQUEST;
                            download.statusCode = StatusCode.ACCEPTED;
                            download.port = receiverPort;
                            download.Send();
                            
                            if (!Directory.Exists(_path + fullFileName +@"\")){
                                Directory.CreateDirectory(_path + fullFileName + @"\");
                            }
                            _server.Stop();
                            DiskHelper.ConsoleWrite("FileReceiver opened");
                            Downloader(fullFileName,receiverPort);
                           
                            _ports.Release(download.port);
                        }

                        if (download.statusCode == StatusCode.FILE_NOT_FOUND){
                            Console.WriteLine("File not found at peer.");
                        }
                    }
                }
            }

            return File.Exists(_path + fullFileName +@"\"+ _hash);;
        }

        private void Stop(){
            _server.Stop();
        }

        public int GetPort(){
            return this._port;
        }

        private void Downloader(string fullFileName, int port){
            var server = new TcpListener(this._ip, port);
            try{
                server.AllowNatTraversal(true);
                server.Start();
            }
            catch (Exception e){
                Logger.Error(e);
                return;
            }
            var client = server.AcceptTcpClient();
            client.ReceiveTimeout = 5000;
            using (NetworkStream stream = client.GetStream()){
                using (var fileStream = File.Open(_path + fullFileName + @"\" + _hash,
                    FileMode.OpenOrCreate, FileAccess.Write)){
                    DiskHelper.ConsoleWrite("Creating file: " + this._hash);

                    int i;
                    while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                        fileStream.Write(_buffer, 0, (i < _buffer.Length) ? i : _buffer.Length);
                    }

                    DiskHelper.ConsoleWrite(@"File done downloading");
                    fileStream.Close();
                }
                stream.Close();
                _ports.Release(port);
            }
        }
    }
}