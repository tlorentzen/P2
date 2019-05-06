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
                    foreach (var hash in _hashList.GetEntry(item)){
                        List<string> receiversOfTheFileHash = _locationDb[hash];
                        Console.WriteLine(_locationDb.Count);
                        foreach (var currentReceiverHash in receiversOfTheFileHash){

                            if (_peers.TryGetValue(currentReceiverHash, out Peer currentReceiver)){
                                FileDeletionMessage deletionMessage = new FileDeletionMessage(currentReceiver);
                                deletionMessage.type = TypeCode.REQUEST;
                                deletionMessage.statusCode = StatusCode.OK;
                                deletionMessage.fileHash = hash;
                                deletionMessage.fullFileHash =  item;
                                deletionMessage.Send();
                            }
                        }
                    }
                }
                _waitHandle.Set();
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