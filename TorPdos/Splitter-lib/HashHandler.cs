using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Splitter_lib{
    public class HashHandler{
       
        private ConcurrentDictionary<string, List<string>> _hashList = new ConcurrentDictionary<string, List<string>>();
        private readonly string _filePath;

        /// <summary>
        /// Make a path to the hashlist in hidden folder
        /// </summary>
        /// <param name="inputPath">Path to add to hashlist.</param>
        public HashHandler(string inputPath){
            string path;
            if (Directory.Exists(inputPath + @"\.hidden\")){
                path = inputPath + @"\.hidden\";
            } else{
                throw new DirectoryNotFoundException();
            }
            _filePath = path + @"hashList.json";
            Load();
        }

        /// <summary>
        /// Loads in the file with the original hash and its splitted parts.
        /// </summary>
        private void Load(){
            //If the file doesn't exist the file will be created
            if (!File.Exists(_filePath)){
                using (FileStream fileCreate = new FileStream(_filePath,FileMode.OpenOrCreate)){
                    fileCreate.Close();
                }
                //If the file exists the information will be loaded in from the file
            } else{
                string json = File.ReadAllText(_filePath);
                if (!string.IsNullOrEmpty(json)){
                    _hashList = JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<string>>>(json);
                }
            }
        }

        /// <summary>
        /// Gets the hashed value out of the file in a list.
        /// </summary>
        /// <param name="fileName">File to to het entry of.</param>
        /// <returns></returns>
        public List<string> GetEntry(string fileName){
            List< string> output = new List<string>();
            if (_hashList.TryGetValue(fileName, out output)){
                return output;
            } else{
                return null;
            }
        }

        /// <summary>
        /// Adds the splitted file hashes to the hashed file list
        /// Takes the splitted file hash and the list it has to be added to as inputs.
        /// </summary>
        /// <param name="hash">Original file hash.</param>
        /// <param name="splittedFileHashes">List of chunk hashes.</param>
        public void Add(string hash, List<string> splittedFileHashes){
            _hashList.TryAdd(hash,splittedFileHashes);
        }

        /// <summary>
        /// Saves the list to the json file
        /// </summary>
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