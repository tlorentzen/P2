using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace P2P_lib
{
    public class P2PConcurrentQueue<T> : ConcurrentQueue<T>
    {
        public delegate void FileQueued();
        public event FileQueued FileAddedToQueue;

        public new void Enqueue(T item){
            base.Enqueue(item);
            FileAddedToQueue.Invoke();
        }
    }
}
