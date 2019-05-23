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

        /// <summary>
        /// Enqueues an item to the queue.
        /// </summary>
        /// <param name="item">The item to be enqueued.</param>
        public new void Enqueue(T item){
            base.Enqueue(item);
            if (ElementAddedToQueue != null)
                ElementAddedToQueue.Invoke();
        }

        /// <summary>
        /// Loads a queue saved as a JSON-file into the program.
        /// </summary>
        /// <param name="path">The path to the queue saved as a JSON-file.</param>
        /// <returns>Returns the loaded Queue.</returns>
        public static StateSaveConcurrentQueue<T> Load(string path){
            var settings = new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Objects};

            if (File.Exists(path)){
                Console.WriteLine($"PATH           {path}");
                string input = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<StateSaveConcurrentQueue<T>>(input, settings);
            }

            return new StateSaveConcurrentQueue<T>();
        }

        /// <summary>
        /// Saves the queue to a JSON-file.
        /// </summary>
        /// <param name="path">The path to where the file should be saved.</param>
        /// <returns>True, when the file is saved.</returns>
        public bool Save(string path){
            var settings = new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Objects,Formatting = Formatting.Indented};
            string output = JsonConvert.SerializeObject(this, settings);
            
            using (var fileStream = new FileStream(path, FileMode.Create)){
                byte[] jsonIndex = new UTF8Encoding(true).GetBytes(output);
                fileStream.Write(jsonIndex, 0, jsonIndex.Length);
                fileStream.Close();
            }

            return true;
        }

        /// <summary>
        /// Enqueues a file to queue.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(T item){
            base.Enqueue(item);
        }

        /// <summary>
        /// ICollection demands this method.
        /// </summary>
        public void Clear(){ }

        /// <summary>
        /// ICollection demands this method.
        /// </summary>
        public bool Contains(T item){
            return false;
        }

        /// <summary>
        /// ICollection demands this method.
        /// </summary>
        public bool Remove(T item){
            return true;
        }
    }
}