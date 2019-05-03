using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Splitter_lib{
    public class HashHandler{
        private ConcurrentDictionary<string, List<string>> _hashList = new ConcurrentDictionary<string, List<string>>();
        private readonly string _filePath;

        public HashHandler(string inputPath){
            string path;
            if (Directory.Exists(inputPath + @"\.hidden\")){
                path = inputPath + @"\.hidden\";
            } else{
                throw new DirectoryNotFoundException();
            }
            _filePath = path + @"hashList.json";
            Start();
        }

        private void Start(){
            Load();
        }

        private void Load(){
            if (!File.Exists(_filePath)){
                using (FileStream fileCreate = new FileStream(_filePath,FileMode.OpenOrCreate)){
                    fileCreate.Close();
                }
            } else{
                string json = File.ReadAllText(_filePath);
                if (!string.IsNullOrEmpty(json)){
                    _hashList = JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<string>>>(json);
                }
            }
        }

        public List<string> GetEntry(string fileName){
            List< string> output = new List<string>();
            if (_hashList.TryGetValue(fileName, out output)){
                return output;
            } else{
                throw new KeyNotFoundException();
            }
        }

        public void Add(string hash, List<string> splittedFileHashes){
            _hashList.TryAdd(hash,splittedFileHashes);
        }


        public void Save(){
            if (_filePath != null){
                string json = JsonConvert.SerializeObject(_hashList);

                using (var fileStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write)){
                    byte[] jsonHashList = new UTF8Encoding(true).GetBytes(json);
                    fileStream.Write(jsonHashList, 0, jsonHashList.Length);
                    fileStream.Close();
                }
            }
        }
    }
}