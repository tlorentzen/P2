using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using P2P_lib.Messages;
using Index_lib;
using Compression;
using Encryption;
using System.Security.Cryptography;

namespace P2P_lib{
    public class NetworkProtocols{
        private NetworkPorts _port = new NetworkPorts();
        private Index Index { get; set; }
        private Network Network { get; set; }
        private HiddenFolder _hiddenFolder;
        public NetworkProtocols(Index index, Network network){
            Index = index;
            Network = network;
            _hiddenFolder = new HiddenFolder(Index.GetPath() + @"\.hidden\");
        }

        //This is the function called to upload a file to the network
        //It takes a path to the file, the number of copies and a seed
        //The seed is to ensure that the same nodes doesn't end up with the files every time
        public void UploadFileToNetwork(string filePath, int copies, int seed = 0){
            //This keeps the number of copies between 0 and 50
            copies = (copies < 0 ? 0 : (copies < 50 ? copies : 50));

            string hash = makeFileHash(filePath);

            //Then the current time is found, to create a unique name for the temporary files
            DateTime utc = DateTime.UtcNow;
            int time = utc.Millisecond + utc.Second * 10 + utc.Minute * 100 + utc.Hour * 1000 + utc.Month * 10000;

            //Then a path is made for the temporary files
            string compressedFilePath = Index.GetPath() + ".hidden\\" + time.ToString();

            Console.WriteLine("File to compress: {0} to path: {1}", filePath, compressedFilePath);

            //The file is then compressed
            ByteCompressor.CompressFile(filePath, compressedFilePath);

            //And then encrypted
            FileEncryption encryption = new FileEncryption(compressedFilePath, ".lzma");
            encryption.DoEncrypt("password");
            HiddenFolder.RemoveFile(compressedFilePath + ".lzma");
            string readyFile = compressedFilePath + ".aes";
            Console.WriteLine(@"File is ready for upload");

            //A copy of the compressed and encrypted file is then send to the set number of peers
            for (int i = 0; i < copies; i++){
                Task.Factory.StartNew(() => SendUploadRequest(readyFile, hash, seed + i));
            }
        }

        private void SendUploadRequest(string filePath, string hash, int seed = 0){
            List<Peer> peerlist = Network.GetPeerList();
            //Console.WriteLine("Will send to "+peerlist.Count+" peers");

            if(peerlist.Count > 0) {
                //seed = seed % peerlist.Count;

                for (int i = 0; i < peerlist.Count; i++){

                    if (peerlist[i].IsOnline()) {
                        Console.WriteLine("Sending to: "+peerlist[i].GetIP());
                        UploadMessage upload = new UploadMessage(peerlist[i]);
                        upload.filesize = new FileInfo(filePath).Length;
                        upload.filename = new FileInfo(filePath).Name;
                        upload.filehash = hash;
                        upload.path = filePath;
                        upload.Send();
                    }

                    //Console.WriteLine(peerlist[i].);

                    
                }
            }
        }
        private string makeFileHash(string filePath) {
            using (var md5 = MD5.Create()) {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    var hash = md5.ComputeHash(fs);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
        }
    }
}
