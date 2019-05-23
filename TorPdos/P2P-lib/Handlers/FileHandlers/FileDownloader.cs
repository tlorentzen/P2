using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using P2P_lib.Helpers;
using P2P_lib.Messages;

namespace P2P_lib{
    [Serializable]
    public class FileDownloader{
        private string _hash;
        private List<string> _peersToAsk;
        private int _port;
        private readonly IPAddress _ip;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("FileDownloader");
        private readonly string _path;
        private readonly byte[] _buffer;
        private NetworkPorts _ports;
        private ConcurrentDictionary<string, Peer> _peers;

        public FileDownloader(NetworkPorts ports, ConcurrentDictionary<string, Peer> peers, int bufferSize = 1024){
            _ports = ports;
            this._ip = IPAddress.Any;
            this._path = DiskHelper.GetRegistryValue("Path") + @".hidden\incoming\";
            this._buffer = new byte[bufferSize];
            this._peers = peers;
        }

        /// <summary>
        /// Fetching function, this fetches the given chunk from the network, returns true if the chunk is fetched.
        /// </summary>
        /// <param name="chunk">The chunk wanted to download.</param>
        /// <param name="fullFileName">The name of the full file.</param>
        /// <returns>Rather the chunk has been fetched.</returns>
        public bool Fetch(P2PChunk chunk, string fullFileName){
            _port = _ports.GetAvailablePort();
            _hash = chunk.hash;
            _peersToAsk = chunk.peers;
            Listener listener = new Listener(this._port);

            foreach (var Peer in _peersToAsk){
                if (!_peers.TryGetValue(Peer, out Peer currentPeer)){
                    break;
                }

                if (currentPeer.IsOnline()){
                    var download = new DownloadMessage(currentPeer){
                        port = this._port,
                        fullFileName = chunk.originalHash,
                        filehash = _hash
                    };

                    // Sends the download message and waits for a
                    // "response" download message to be received.
                    // Then changed 'download' to this message and
                    // returns true. If a response is not received
                    // within time, it returns false.
                    if (listener.SendAndAwaitResponse(ref download, 3000)){
                        // If the download is accepted, a receiver is
                        // started and the port of the receiver is
                        // sent to the peer.
                        if (download.statusCode == StatusCode.ACCEPTED){
                            int receiverPort = _ports.GetAvailablePort();
                            download.CreateReply();
                            download.type = Messages.TypeCode.REQUEST;
                            download.statusCode = StatusCode.ACCEPTED;
                            download.port = receiverPort;

                            if (!Directory.Exists(_path + fullFileName + @"\")){
                                Directory.CreateDirectory(_path + fullFileName + @"\");
                            }

                            download.Send();
                            
                            if (!currentPeer.IsOnline()){
                                DiskHelper.ConsoleWrite("The peer requested went offline.");
                                continue;
                            }
                            DiskHelper.ConsoleWrite("FileReceiver opened");
                            if (!Downloader(fullFileName, receiverPort)){
                                return false;
                            }
                            
                            _ports.Release(download.port);
                            break;
                        } else if (download.statusCode == StatusCode.FILE_NOT_FOUND){
                            Console.WriteLine("File not found at peer.");
                            chunk.peers.Remove(download.fromUuid);
                            //TODO Remove peer from location DB
                        }
                    }
                }
            }

            if (File.Exists(_path + fullFileName + @"\" + _hash)){
                if (CheckDownloadHash(_path + fullFileName + @"\" + _hash,_hash)){
                    DiskHelper.ConsoleWrite(@"Chunk done downloading");
                    return true;
                }   
            }

            return false;
        }
        
        private bool CheckDownloadHash(string filename,string inputHash)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    string stringHash = BitConverter.ToString(hash).Replace("-", "");
                    return  stringHash == inputHash;
                }
            }
        }

        /// <summary>
        /// This is a helper function for the fetching function, this is responsible for downloading the chunk.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <param name="port">Port for which to download from.</param>
        private bool Downloader(string fullFileName, int port){

            int timeout = 3000;
            int timeout_counter = 0;
            var server = new TcpListener(this._ip, port);

            try{
                server.AllowNatTraversal(true);
                server.Server.ReceiveTimeout = 1000;
                server.Server.SendTimeout = 1000;
                server.Start();
            }catch (Exception e){
                Logger.Error(e);
                return false;
            }

            while (!server.Pending())
            {
                if (timeout_counter >= timeout)
                {
                    server.Stop();
                    return false;
                }
                timeout_counter++;
                Thread.Sleep(5);
            }

            try
            {
                var client = server.AcceptTcpClient();
                client.ReceiveTimeout = 1000;
                client.Client.ReceiveTimeout = 1000;
                
                using (NetworkStream stream = client.GetStream()){
                    using (var fileStream = File.Open(_path + fullFileName + @"\" + _hash,
                        FileMode.OpenOrCreate, FileAccess.Write)){
                        DiskHelper.ConsoleWrite("Creating file: " + this._hash);

                        int i;
                        while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                            fileStream.Write(_buffer, 0, (i < _buffer.Length) ? i : _buffer.Length);
                        }

                        
                        fileStream.Close();
                    }

                    stream.Close();
                    _ports.Release(port);
                    return true;
                }
            }
            catch (Exception e){
                DiskHelper.ConsoleWrite("The peer requested went offline in Downloader." + e);
                return false;
            }
        }
    }
}