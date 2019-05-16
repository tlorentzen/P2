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
using System.Linq;

namespace P2P_lib
{
    class FileUploader
    {
        private TcpListener _server;
        private Thread _listener;
        private int _port;
        private string _path;
        private readonly IPAddress _ip;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("FileUploader");
        private readonly byte[] _buffer;
        private readonly Receiver _receiver;
        private NetworkPorts _ports;
        private ConcurrentDictionary<string, Peer> _peers;

        public FileUploader(NetworkPorts ports, ConcurrentDictionary<string, Peer> peers, int bufferSize = 1024)
        {
            this._ports = ports;
            this._ip = IPAddress.Any;
            this._path = DiskHelper.GetRegistryValue("Path");
            this._buffer = new byte[bufferSize];
            this._peers = peers;
        }

        public bool push(P2PChunk chunk, string chunk_path, int num_of_receving_peers=10)
        {
            this._port = _ports.GetAvailablePort();
            List<Peer> peers = this.GetPeers(num_of_receving_peers);
            FileInfo fileInfo = new FileInfo(chunk_path);
            Listener listner = new Listener(this._port);
            Boolean sendToAll = true;

            foreach (Peer peer in peers){

                var upload = new UploadMessage(peer)
                {
                    filesize = fileInfo.Length,
                    filename = chunk.Hash,
                    filehash = chunk.Hash,
                    path = chunk_path,
                    port = this._port
                };

                if(listner.SendAndAwaitResponse(ref upload, 2000))
                {
                    if(upload.statusCode == StatusCode.ACCEPTED){
                        FileSender sender = new FileSender(peer.StringIp, upload.port);

                        if(sender.Send(chunk.Path(chunk_path))){
                            chunk.AddPeer(peer.GetUuid());
                        }else{
                            sendToAll = false;
                        }
                    }
                }
            }

            _ports.Release(_port);
            return sendToAll;
        }

        private List<Peer> GetPeers(int count)
        {
            List<Peer> topPeers = _peers.Values.Where(peer => peer.IsOnline() == true).ToList<Peer>();
            topPeers.Sort(new ComparePeersByRating());
            if (topPeers.Count > 0)
            {
                int wantedLengthOfTopList = Math.Min(topPeers.Count, count);
                topPeers.RemoveRange(wantedLengthOfTopList, Math.Max(0, topPeers.Count - wantedLengthOfTopList));
            }

            return topPeers;
        }
    }
}
