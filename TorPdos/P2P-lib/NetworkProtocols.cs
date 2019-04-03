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

namespace P2P_lib{
    public class NetworkProtocols{
        Receiver receiver;
        private NetworkPorts port = new NetworkPorts();
        private Index _index { get; set; }
        private Network _network { get; set; }
        private List<string> _recieverList = new List<string>();
        public NetworkProtocols(Index index, Network network){
            _index = index;
            _network = network;
        }

        //This is the function called to upload a file to the network
        //It takes a path to the file, the number of copies and a seed
        //The seed is to ensure that the same nodes doesn't end up with the files every time
        public void UploadFileToNetwork(string filePath, int copies, int seed = 0){
            //This keeps the number of copies between 0 and 50
            copies = (copies < 0 ? 0 : (copies < 50 ? copies : 50));

            //Then the current time is found, to create a unique name for the temporary files
            DateTime utc = DateTime.UtcNow;
            int time = utc.Millisecond + utc.Second * 10 + utc.Minute * 100 + utc.Hour * 1000 + utc.Month * 10000;

            //Then a path is made for the temporary files
            string compressedFilePath = _index.GetPath() + "\\.hidden\\" + time.ToString();

            //The file is then compressed
            ByteCompressor.CompressFile(filePath, compressedFilePath);

            //And then encrypted
            FileEncryption encryption = new FileEncryption(compressedFilePath, ".lzma");
            encryption.doEncrypt("password");
            //HiddenFolder hiddenFolder = new HiddenFolder(_index.GetPath());
            //hiddenFolder.Remove(compressedFilePath + ".lzma");
            string readyFile = compressedFilePath + ".aes";
            Console.WriteLine("File is ready for upload");

            //A copy of the compressed and encrypted file is then send to peers
            for (int i = 0; i < copies; i++){
                Task.Factory.StartNew(() => SendUploadRequest(readyFile, seed + i));
            }
        }

        private void SendUploadRequest(string filePath, int seed = 0){
            List<Peer> peerlist = _network.getPeerList();
            seed = seed % peerlist.Count;
            UploadMessage upload = new UploadMessage(/*peerlist[seed].GetIP()*/ "192.168.0.109");
            upload.filesize = new FileInfo(filePath).Length;
            upload.filename = new FileInfo(filePath).Name;
            upload.from = NetworkHelper.getLocalIPAddress();
            upload.filehash = DiskHelper.CreateMD5(filePath);
            upload.path = filePath;
            upload.type = Messages.TypeCode.REQUEST;
            upload.statuscode = StatusCode.OK;
            upload.port = port.GetAvailablePort();
            Console.WriteLine("Filesize: " + upload.filesize);
            Console.WriteLine("Filename: " + upload.filename);
            Console.WriteLine("From: " + upload.from);
            Console.WriteLine("Filepath: " + upload.path);
            receiver = new Receiver(upload.port);
            receiver.start();
            Console.WriteLine("Receiver on port: " + upload.port);
            receiver.MessageReceived += Receiver_MessageReceived;
            upload.Send();
            Console.WriteLine("Upload request sent");
        }

        private void Receiver_MessageReceived(BaseMessage msg){
            Console.WriteLine("Message received");
            if (msg.GetMessageType() == typeof(UploadMessage)){
                UploadMessage upload = (UploadMessage) msg;
                Console.WriteLine("Upload message received");

                if (upload.type.Equals(Messages.TypeCode.REQUEST)){
                    if (DiskHelper.GetTotalFreeSpace("C:\\") > upload.filesize){
                        upload.statuscode = StatusCode.ACCEPTED;
                    } else{
                        upload.statuscode = StatusCode.INSUFFICIENT_STORAGE;
                    }

                    upload.CreateReply();
                    upload.Send();
                } else if (upload.type.Equals(Messages.TypeCode.RESPONSE)){
                    Console.WriteLine("This is an upload response");
                    if (upload.statuscode == StatusCode.ACCEPTED){
                        Console.WriteLine("It's accepted");
                        IndexFile indexFile = _index.GetEntry(upload.filehash);
                        string filePath = indexFile.getPath();
                        Console.WriteLine("Path from indexfile is: " + indexFile.getPath());
                        FileSender fileSender = new FileSender(upload.from, upload.port);
                        Console.WriteLine("Upload is send from: " + upload.from + " and file vil be sent to: " + upload.port);
                        fileSender.Send(filePath);
                        Console.WriteLine(filePath + " has been sent");
                        HiddenFolder hiddenFolder = new HiddenFolder(_index.GetPath());
                        hiddenFolder.Remove(filePath);
                    }
                }
            }
        }
    }
}
