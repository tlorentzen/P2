using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace P2P_lib
{
    public class DownloadManager
    {

        private NetworkPorts _ports;
        private BlockingCollection<Peer> _peers;
        private P2PConcurrentQueue<QueuedFile> _queue;

        public DownloadManager(P2PConcurrentQueue<QueuedFile> queue, NetworkPorts ports, BlockingCollection<Peer> peers)
        {
            this._queue = queue;
            this._ports = ports;
            this._peers = peers;
        }

        public void Run()
        {
            
        }

    }
}
