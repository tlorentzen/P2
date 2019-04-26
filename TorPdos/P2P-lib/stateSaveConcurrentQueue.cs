using System.Collections.Concurrent;
using System.IO;

namespace P2P_lib
{
    public class StateSaveConcurrentQueue<T> : ConcurrentQueue<T>
    {
        public delegate void FileQueued();
        public event FileQueued ElementAddedToQueue;
        private string _path;

        /// <summary>
        /// Takes full path to the file.
        /// </summary>
        /// <param name="path">Full file path</param>
        public StateSaveConcurrentQueue(string path){
            _path = path;
            load();
        }

        public new void Enqueue(T item){
            base.Enqueue(item);
            if (ElementAddedToQueue != null) ElementAddedToQueue.Invoke();
        }

        private void load(){
            if (File.Exists(_path)){
                string input =File.ReadAllText(_path);
                
            }
            
        }
    }
}
