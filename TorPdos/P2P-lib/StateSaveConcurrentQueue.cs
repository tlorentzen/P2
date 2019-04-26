using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace P2P_lib{
    public class StateSaveConcurrentQueue<T> : ConcurrentQueue<T>{
        public delegate void ElementQueued();

        public event ElementQueued ElementAddedToQueue;

        private readonly string _path;
        private ConcurrentQueue<T> _concurrentQueue;

        /// <summary>
        /// Takes full path to the file.
        /// </summary>
        /// <param name="path">Full file path</param>
        public StateSaveConcurrentQueue(string path){
            _path = path;
            if (!Load()){
                _concurrentQueue = new ConcurrentQueue<T>();
            }
        }

        public new void Enqueue(T item){
            base.Enqueue(item);
            if (ElementAddedToQueue != null) ElementAddedToQueue.Invoke();
        }

        public bool Load(){
            if (File.Exists(_path)){
                Console.WriteLine("PATH           "+ _path);
                string input = File.ReadAllText(_path);
                _concurrentQueue = JsonConvert.DeserializeObject<ConcurrentQueue<T>>(input);
                return true;
            }

            return false;
        }

        public bool Save(){
                string output = JsonConvert.SerializeObject(_concurrentQueue);
                Console.WriteLine("Got here?");
                Console.WriteLine(_concurrentQueue.Count);
                using (var fileStream = new FileStream(_path, FileMode.OpenOrCreate)){
                    byte[] jsonIndex = new UTF8Encoding(true).GetBytes(output);
                    fileStream.Write(jsonIndex, 0, jsonIndex.Length);
                    fileStream.Close();
                }

                return true;
        }
    }
}