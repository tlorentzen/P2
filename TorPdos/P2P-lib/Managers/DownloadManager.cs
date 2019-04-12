using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace P2P_lib
{
    public class DownloadManager
    {
        private ManualResetEvent _waitHandle;
        private bool is_running = true;
        private NetworkPorts _ports;
        private BlockingCollection<Peer> _peers;
        private P2PConcurrentQueue<QueuedFile> _queue;

        public DownloadManager(P2PConcurrentQueue<QueuedFile> queue, NetworkPorts ports, BlockingCollection<Peer> peers)
        {
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;

            this._waitHandle = new ManualResetEvent(false);
            this._queue.FileAddedToQueue += _queue_FileAddedToQueue;
        }

        private void _queue_FileAddedToQueue()
        {
            this._waitHandle.Set();
        }

        public void Run()
        {
            while (is_running)
            {
                this._waitHandle.WaitOne();

                QueuedFile file;

                while (this._queue.TryDequeue(out file))
                {
                    //FileReceiver receiver = new FileReceiver();

                    Console.WriteLine(file.GetHash());
                }

                this._waitHandle.Reset();
            }
        }

    }
}
