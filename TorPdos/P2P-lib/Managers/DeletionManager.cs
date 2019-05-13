using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using P2P_lib.Messages;
using Splitter_lib;
using TypeCode = P2P_lib.Messages.TypeCode;

namespace P2P_lib.Managers{
    public class DeletionManager : Manager{
        private bool _isRunning = true;
        private int _port;
        private readonly NetworkPorts _ports;
        private readonly ManualResetEvent _waitHandle;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly StateSaveConcurrentQueue<string> _queue;
        private readonly ConcurrentDictionary<string, List<string>> _locationDb;
        public bool isStopped;
        private readonly HashHandler _hashList;

        public DeletionManager(StateSaveConcurrentQueue<string> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, ConcurrentDictionary<string, List<string>> locationDb,
            HashHandler hashList){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
            this._waitHandle = new ManualResetEvent(false);
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            this._locationDb = locationDb;
            this._hashList = hashList;
        }

        private void QueueElementAddedToQueue(){
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

                    if (_hashList == null){
                        _queue.Enqueue(item);
                        return;
                    }

                    List<string> currentHashList = _hashList.GetEntry(item);
                    if (currentHashList == null){
                        return;
                    }
                    foreach (var hash in currentHashList){
                        

                        _locationDb.TryGetValue(hash, out var receiversOfTheFileUuid);

                        if (receiversOfTheFileUuid == null) continue;

                        foreach (var currentReceiverUuid in receiversOfTheFileUuid){
                            if (!_peers.TryGetValue(currentReceiverUuid, out Peer currentReceiver)) continue;

                            var deletionMessage = new FileDeletionMessage(currentReceiver){
                                type = TypeCode.REQUEST,
                                statusCode = StatusCode.OK,
                                fileHash = hash,
                                fullFileHash = item
                            };
                            deletionMessage.Send();
                        }
                    }
                }

                _waitHandle.Reset();
            }

            isStopped = true;
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