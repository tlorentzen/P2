using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Index_lib;
using Microsoft.Win32;

namespace LoggingThreadder{
    public class LoggingManager{
        private string _path;
        private HiddenFolder _hiddenFolder;
        private LoggingQueueHelper<string> _queue;
        private RegistryKey myrest = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
        private ManualResetEvent waitHandle;

        public LoggingManager(){
            this.waitHandle = new ManualResetEvent(false);
            this._queue.LogAddedToQueue += _queue_LogAddedToQueue;
                this._path = myrest.GetValue("Path").ToString();
            _hiddenFolder = new HiddenFolder(this._path + @"\.hidden\");
        }

        public void add(string){
            
        }
        
        private void _queue_LogAddedToQueue()
        {
            this.waitHandle.Set();
        }
        
        
    }

    public class LoggingQueueHelper<T> : ConcurrentQueue<T>{
        public delegate void logQueued();

        public event logQueued LogAddedToQueue;

        public void Enqueue(T item){
            base.Enqueue(item);
            this.LogAddedToQueue();
        }
    }
}