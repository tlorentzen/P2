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
            _waitHandle = new ManualResetEvent(false);
            _queue.ErrorAddedToQueue += _queue_ErrorAddedToQueue;
            _source = source;
        }

        private void _queue_ErrorAddedToQueue(){
            _waitHandle.Set();
        }

        public void run(){
            while (is_running){
                _waitHandle.WaitOne();
                string output;
                while (_queue.TryDequeue(out output)){
                    string path = _registry.GetValue("Path").ToString();
                    
                    _hiddenFolder = new HiddenFolder(path + @"\.hidden");
                    _hiddenFolder.appendToFileLog(path + @"\.hidden\log" + _source + ".txt", output);
                }

                _waitHandle.Reset();
            }
        }
    }
}