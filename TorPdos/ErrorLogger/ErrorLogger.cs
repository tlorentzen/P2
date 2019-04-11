using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using Index_lib;
using Microsoft.Win32;

namespace ErrorLogger{
    public class ErrorLoggerQueue{
        private ErrorQueueHandler<string> _queue;
        private HiddenFolder _hiddenFolder;
        private readonly RegistryKey _registry = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
        private readonly ManualResetEvent _waitHandle;
        private bool is_running = true;
        private readonly string _source;

        public ErrorLoggerQueue(ErrorQueueHandler<string> queue, string source){
            _queue = queue;
            this._waitHandle = new ManualResetEvent(false);
            this._queue.ErrorAddedToQueue += _queue_ErrorAddedToQueue;
            this._source = source;
        }

        private void _queue_ErrorAddedToQueue(){
            this._waitHandle.Set();
        }

        public void run(){
            while (is_running){
                this._waitHandle.WaitOne();
                string output;
                while (this._queue.TryDequeue(out output)){
                    string path = _registry.GetValue("Path").ToString();
                    
                    _hiddenFolder = new HiddenFolder(path + @"\.hidden");
                    _hiddenFolder.AppendToFileLog(path + @"\.hidden\log" + _source + ".txt", output);
                }

                this._waitHandle.Reset();
            }
        }
    }
}