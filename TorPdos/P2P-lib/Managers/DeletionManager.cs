using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using P2P_lib.Messages;
using TypeCode = P2P_lib.Messages.TypeCode;

namespace P2P_lib.Managers{
    public class DeletionManager : Manager{
        private bool is_running = true;
        private int _port;
        private string _filehash;
        private string _path;
        private NetworkPorts _ports;
        private readonly ManualResetEvent _waitHandle;
        private readonly ConcurrentDictionary<string, Peer> _peers;
        private readonly StateSaveConcurrentQueue<string> _queue;
        private readonly ConcurrentDictionary<string, List<string>> _locationDb;
        private FileReceiver _receiver;
        public bool isStopped;

        public DeletionManager(StateSaveConcurrentQueue<string> queue, NetworkPorts ports,
            ConcurrentDictionary<string, Peer> peers, ConcurrentDictionary<string, List<string>> locationDB){
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
            this._path = DiskHelper.getRegistryValue("Path").ToString();
            this._waitHandle = new ManualResetEvent(false);
            this._queue.ElementAddedToQueue += QueueElementAddedToQueue;
            this._port = _ports.GetAvailablePort();
            this._locationDb = locationDB;
        }

        private void QueueElementAddedToQueue(){
            this._waitHandle.Set();
        }

        public void Run(){
            isStopped = false;
            while (is_running){
                if (!is_running){
                    break;
                }

                this._waitHandle.WaitOne();
                string item;
                while (this._queue.TryDequeue(out item)){
                    if (!is_running){
                        _waitHandle.Set();
                        break;
                    }

                    List<string> inputlist = _locationDb[item];
                    Console.WriteLine(_locationDb.Count);
                    foreach (var input in inputlist){
                        Console.WriteLine(_peers.Count);
                        if (_peers.TryGetValue(input, out Peer value)){
                            FileDeletionMessage deletionMessage = new FileDeletionMessage(value);
                            deletionMessage.type = TypeCode.REQUEST;
                            deletionMessage.statuscode = StatusCode.OK;
                            deletionMessage.filehash = item;
                            deletionMessage.port = _port;
                            deletionMessage.Send();
                        }
                    }
                }
            }

            isStopped = true;
        }


        public override bool Shutdown(){
            is_running = false;
            _waitHandle.Set();

            Console.Write("Deletion thread stopping... ");
            while (!this.isStopped){ }

            Console.Write("Stopped!");

            return true;
        }
    }
}