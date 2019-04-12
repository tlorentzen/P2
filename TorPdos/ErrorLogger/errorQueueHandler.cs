using System.Collections.Concurrent;

namespace ErrorLogger{
    public class ErrorQueueHandler<T> : ConcurrentQueue<T>{
        public delegate void ErrorQueued();

        public event ErrorQueued ErrorAddedToQueue;

        public void enqueue(T item){
            base.Enqueue(item);
            var onErrorAddedToQueue = ErrorAddedToQueue;
            if (onErrorAddedToQueue != null) onErrorAddedToQueue();
        }
    }
}