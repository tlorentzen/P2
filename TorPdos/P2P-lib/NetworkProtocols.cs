using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using P2P_lib.Messages;
using P2P_lib;
using Index_lib;
using Compression;
using Encryption;
using System.Security.Cryptography;

namespace P2P_lib{
    public class NetworkProtocols{
        private NetworkPorts port = new NetworkPorts();
        private Index _index { get; set; }
        private Network _network { get; set; }
        private HiddenFolder _hiddenFolder;
        public NetworkProtocols(Index index, Network network){
            _index = index;
            _network = network;
            _hiddenFolder = new HiddenFolder(_index.GetPath() + @"\.hidden\");
        }

        //This is the function called to upload a file to the network
        //It takes a path to the file, the number of copies and a seed
        //The seed is to ensure that the same nodes doesn't end up with the files every time
        public string UploadFileToNetwork(string filePath, int copies, int seed = 0){
            //This keeps the number of copies between 0 and 50
            copies = (copies < 0 ? 0 : (copies < 50 ? copies : 50));

            string hash = makeFileHash(filePath);

            //Then the current time is found, to create a unique name for the temporary files
            DateTime utc = DateTime.UtcNow;
            int time = utc.Millisecond + utc.Second * 10 + utc.Minute * 100 + utc.Hour * 1000 + utc.Month * 10000;

            //Then a path is made for the temporary files
            string compressedFilePath = _index.GetPath() + ".hidden\\" + time.ToString();

            Console.WriteLine("File to compress: {0} to path: {1}", filePath, compressedFilePath);

            //The file is then compressed
            ByteCompressor.CompressFile(filePath, compressedFilePath);

            //And then encrypted
            FileEncryption encryption = new FileEncryption(compressedFilePath, ".lzma");
            encryption.doEncrypt("password");
            _hiddenFolder.RemoveFile(compressedFilePath + ".lzma");
            string readyFile = compressedFilePath + ".aes";
            Console.WriteLine("File is ready for upload");

            //A copy of the compressed and encrypted file is then send to the set number of peers
            for (int i = 0; i < copies; i++){
                Task.Factory.StartNew(() => SendUploadRequest(readyFile, hash, seed + i));
            }
            return compressedFilePath;
        }
        private void SendUploadRequest(string filePath, string hash, int seed = 0){
            List<Peer> peerlist = _network.getPeerList();
            if(peerlist.Count > 0) {
                seed = seed % peerlist.Count;
                UploadMessage upload = new UploadMessage(/*peerlist[seed].GetIP()*/ "192.168.0.100");
                upload.filesize = new FileInfo(filePath).Length;
                upload.filename = new FileInfo(filePath).Name;
                upload.filehash = hash;
                Console.WriteLine("Filehash: {0}", upload.filehash);
                upload.path = filePath;
                upload.type = Messages.TypeCode.REQUEST;
                upload.statuscode = StatusCode.OK;
                Console.WriteLine("Filesize: {0}", upload.filesize);
                Console.WriteLine("Filename: {0}", upload.filename);
                Console.WriteLine("From: {0}", upload.from);
                Console.WriteLine("Filepath: {0}", upload.path);
                upload.Send();
                Console.WriteLine("Upload request sent");
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
