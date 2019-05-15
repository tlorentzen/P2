using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace P2P_lib{
    [Serializable]
    public class FileDownloader{
        [JsonProperty] private string _hash;
        [JsonProperty] private int _chunkCount;
        [JsonProperty] private int _receivedChunkCount = 0;
        [JsonProperty] private List<string> _chunkHashes;
        [JsonProperty] ConcurrentDictionary<string, List<string>> _peersWithFile;

        public FileDownloader(string hash){
            _hash = hash;
        }

        [JsonConstructor]
        private FileDownloader(string hash, int chunkCount, List<string> chunkHashes,
            ConcurrentDictionary<string, List<string>> peersWithFile){
            _hash = hash ?? throw new ArgumentNullException(nameof(hash));
            _chunkCount = chunkCount;
            _chunkHashes = chunkHashes ?? throw new ArgumentNullException(nameof(chunkHashes));
            _peersWithFile = peersWithFile ?? throw new ArgumentNullException(nameof(peersWithFile));
        }

        public void UpdateInformation(List<string> chunkHashes,
            ConcurrentDictionary<string, List<string>> peersWithFile){
            _receivedChunkCount = chunkHashes.Count;
            _chunkHashes = chunkHashes;
            _peersWithFile = peersWithFile;
        }

        public bool FileSuccesfullyDownloaded(){
            _receivedChunkCount++;
            return _receivedChunkCount == _chunkCount;
        }

        public List<string> GetChunkHosts(string chunkHash){
            _peersWithFile.TryGetValue(chunkHash, out var chunkHashList);
            return chunkHashList;
        }

        public string GetHash(){
            return _hash;
        }
    }
}