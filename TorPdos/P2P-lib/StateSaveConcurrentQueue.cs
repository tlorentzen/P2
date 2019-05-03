using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace P2P_lib{
    [Serializable]
    public class StateSaveConcurrentQueue<T> : ConcurrentQueue<T>, ICollection<T>{
        public bool IsReadOnly => false;

        public delegate void ElementQueued();

        public event ElementQueued ElementAddedToQueue;

        public new void Enqueue(T item){
            base.Enqueue(item);
            if (ElementAddedToQueue != null)
                ElementAddedToQueue.Invoke();
        }

        public static StateSaveConcurrentQueue<T> Load(string path){
            var settings = new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Objects};

            if (File.Exists(path)){
                Console.WriteLine("PATH           " + path);
                string input = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<StateSaveConcurrentQueue<T>>(input, settings);
            }

            return new StateSaveConcurrentQueue<T>();
        }

        public bool Save(string path){
            var settings = new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Objects,Formatting = Formatting.Indented};
            string output = JsonConvert.SerializeObject(this, settings);
            Console.WriteLine("Queue size: " + this.Count);
            using (var fileStream = new FileStream(path, FileMode.Create)){
                byte[] jsonIndex = new UTF8Encoding(true).GetBytes(output);
                fileStream.Write(jsonIndex, 0, jsonIndex.Length);
                fileStream.Close();
            }

            return true;
        }

        public void Add(T item){
            base.Enqueue(item);
        }

        public void Clear(){ }

        public bool Contains(T item){
            return false;
        }

        public bool Remove(T item){
            return true;
        }
    }
}