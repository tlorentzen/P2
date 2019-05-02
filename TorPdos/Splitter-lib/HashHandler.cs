using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Splitter_lib{
    public class HashHandler{
        private ConcurrentDictionary<string, List<string>> HashList = new ConcurrentDictionary<string, List<string>>();
        private string _filePath;
        private string _path;

        public HashHandler(string path){

            if (Directory.Exists(path + @"\.hidden\")){
                _path = path + @"\.hidden\";
            } else{
                throw new DirectoryNotFoundException();
            }
            _filePath = _path + @"hashList.json";
            start();
        }

        private int start(){
            load();
            return 1;
        }

        public bool load(){
            if (!File.Exists(_filePath)){
                using (FileStream fileCreate = new FileStream(_filePath,FileMode.OpenOrCreate)){
                    fileCreate.Close();
                }
            } else{
                string json = File.ReadAllText(_filePath);
                if (!string.IsNullOrEmpty(json)){
                    HashList = JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<string>>>(json);
                }
            }
            return true;
        }

        public List<string> getEntry(string fileName){
            List< string> output = new List<string>();
            if (HashList.TryGetValue(fileName, out output)){
                return output;
            } else{
                throw new KeyNotFoundException();
            }
        }

        public void Add(string hash, List<string> splittedFileHashes){
            HashList.TryAdd(hash,splittedFileHashes);
        }


        public int save(){
            if (_filePath != null){
                string json = JsonConvert.SerializeObject(HashList);

                using (var fileStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write)){
                    byte[] jsonHashList = new UTF8Encoding(true).GetBytes(json);
                    fileStream.Write(jsonHashList, 0, jsonHashList.Length);
                    fileStream.Close();
                }
            }

            return 1;
        }
    }
}