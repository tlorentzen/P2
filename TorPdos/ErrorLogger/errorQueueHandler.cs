using System.Collections.Concurrent;

namespace ErrorLogger{
    public class ErrorQueueHandler<T> : ConcurrentQueue<T>{
        public delegate void ErrorQueued();

        public event ErrorQueued ErrorAddedToQueue;

        public new void Enqueue(T item){
            base.Enqueue(item);
            this.ErrorAddedToQueue();
        }
    }
}