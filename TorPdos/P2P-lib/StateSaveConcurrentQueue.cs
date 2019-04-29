using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace P2P_lib{
    [Serializable]
    public class StateSaveConcurrentQueue<T> : ConcurrentQueue<T>{

        public delegate void ElementQueued();

        public event ElementQueued ElementAddedToQueue;

        private readonly string _path;

        /// <summary>
        /// Takes full path to the file.
        /// </summary>
        /// <param name="path">Full file path</param>
        public StateSaveConcurrentQueue(string path){
            _path = path;
            Load(path);
        }

        public new void Enqueue(T item){
            base.Enqueue(item);
            if (ElementAddedToQueue != null) ElementAddedToQueue.Invoke();
        }

        public static StateSaveConcurrentQueue<T> Load(string path){
            if (File.Exists(path)){
                Console.WriteLine("PATH           "+ path);
                string input = File.ReadAllText(path);
                StateSaveConcurrentQueue<T> output= JsonConvert.DeserializeObject<StateSaveConcurrentQueue<T>>(input);
                return output;
            }

            return new StateSaveConcurrentQueue<T>(path);
        }

        public bool Save(){
                string output = JsonConvert.SerializeObject(this);
                Console.WriteLine("Queue size: " +this.Count);
                using (var fileStream = new FileStream(_path, FileMode.OpenOrCreate)){
                    byte[] jsonIndex = new UTF8Encoding(true).GetBytes(output);
                    fileStream.Write(jsonIndex, 0, jsonIndex.Length);
                    fileStream.Close();
                }

                return true;
        }
    }
}