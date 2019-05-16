using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;


namespace P2P_lib.Managers{
    public class DeletionManager : Manager{
        private bool _isRunning = true;
        private readonly NetworkPorts _ports;
        private readonly ManualResetEvent _waitHandle;
        private readonly StateSaveConcurrentQueue<string> _queue;
        private readonly ConcurrentDictionary<string, P2PFile> _filesList;
        private bool _isStopped;
        private readonly FileDeleter _fileDeleter;
        
        public DeletionManager(StateSaveConcurrentQueue<string> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, ConcurrentDictionary<string, P2PFile> locations){
            this._queue = queue;
            this._ports = ports;
            this._waitHandle = new ManualResetEvent(false);
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            this._filesList = locations;
            Peer.PeerSwitchedOnline += PeerWentOnline;
            _fileDeleter = new FileDeleter(peers,_ports);
        }

        private void QueueElementAddedToQueue(){
            this._waitHandle.Set();
        }

        private void PeerWentOnline(){
            this._waitHandle.Set();
        }

        /// <summary>
        /// This is the runner function, needs to be called when the manager needs to run.
        /// </summary>
        public void Run(){
            _isStopped = false;
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

                _isStopped = true;
            }
        }

        /// <summary>
        /// Shutdown function, used to stop managers.
        /// </summary>
        /// <returns> Returns a boolean, returns true when the manager is stopped.</returns>
        public override bool Shutdown(){
            _isRunning = false;
            _waitHandle.Set();
            Console.Write("Deletion thread stopping... ");
            while (!this._isStopped){ }

            Console.Write("Stopped!");
            return true;
        }
    }
}