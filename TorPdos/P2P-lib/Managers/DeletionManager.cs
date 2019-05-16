using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;


namespace P2P_lib.Managers{
    public class DeletionManager : Manager{
        private bool _isRunning = true;
        private int _port;
        private Thread _listener;
        private readonly NetworkPorts _ports;
        private readonly ManualResetEvent _waitHandle;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly StateSaveConcurrentQueue<string> _queue;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("DeletionManager");
        private ConcurrentDictionary<string, P2PFile> _filesList = new ConcurrentDictionary<string, P2PFile>();
        public bool isStopped;
        private IPAddress _ip;
        private FileDeleter _fileDeleter;

        public DeletionManager(StateSaveConcurrentQueue<string> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, ConcurrentDictionary<string, P2PFile> locations){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
            this._waitHandle = new ManualResetEvent(false);
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            this._filesList = locations;
            _ip = IPAddress.Any;
            Peer.PeerSwitchedOnline += PeerWentOnline;
            _fileDeleter = new FileDeleter(peers,_ports);
        }

        private void QueueElementAddedToQueue(){
            this._waitHandle.Set();
        }

        private void PeerWentOnline(){
            this._waitHandle.Set();
        }

        public void Run(){
            isStopped = false;
            while (_isRunning){
                if (!_isRunning){
                    break;
                }

                this._waitHandle.WaitOne();

                while (this._queue.TryDequeue(out var item)){
                    if (!_isRunning){
                        _waitHandle.Set();
                        break;
                    }
                    
                    _filesList.TryGetValue(item, out var currentFile);
                    if (currentFile == null){
                        return;
                    }

                    foreach (var currentFileChunk in currentFile.Chunks){
                        if (!_fileDeleter.ChunkDeleter(currentFileChunk,currentFile)){
                            _queue.Enqueue(currentFile.Hash);
                        }
                    }

                    _waitHandle.Reset();
                }

                isStopped = true;
            }
        }


        public override bool Shutdown(){
            _isRunning = false;
            _waitHandle.Set();
            Console.Write("Deletion thread stopping... ");
            while (!this.isStopped){ }

            Console.Write("Stopped!");
            return true;
        }
    }
}