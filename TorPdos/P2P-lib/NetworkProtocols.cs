﻿using System;
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

namespace P2P_lib {
    public class NetworkProtocols {
        Receiver receiver;
        private Index _index { get; set; }
        private Network _network { get; set; }
        public NetworkProtocols(Index index, Network network) {
            _index = index;
            _network = network;
        }
        public void UploadFileToNetwork (string filePath, int copies, int seed = 0) {
            copies = (copies < 0 ? 0 : (copies < 50 ? copies : 50));
            for(int i = 0; i < copies; i++) {
                Task.Factory.StartNew(() => SendUploadRequest(filePath, seed + i));
            }
        }

        private void SendUploadRequest(string filePath, int seed = 0) {
            List<Peer> peerlist = _network.getPeerList();
            seed = seed % peerlist.Count - 1;
            UploadMessage upload = new UploadMessage(peerlist[seed].GetIP());
            upload.filesize = new FileInfo(filePath).Length;
            upload.filename = new FileInfo(filePath).Name;
            upload.filehash = DiskHelper.CreateMD5(filePath);
            upload.path = filePath;
            upload.type = Messages.TypeCode.REQUEST;
            upload.statuscode = StatusCode.OK;
            upload.port = NetworkHelper.getAvailablePort(55000, 56000);
            receiver = new Receiver(upload.port);
            receiver.start();
            receiver.MessageReceived += Receiver_MessageReceived;
            upload.Send();
        }

        private void Receiver_MessageReceived(BaseMessage msg) {
            if (msg.GetMessageType() == typeof(UploadMessage)) {
                UploadMessage upload = (UploadMessage)msg;

                if (upload.type.Equals(Messages.TypeCode.REQUEST)) {
                    if (DiskHelper.GetTotalFreeSpace("C:\\") > upload.filesize) {
                        upload.statuscode = StatusCode.ACCEPTED;
                    } else {
                        upload.statuscode = StatusCode.INSUFFICIENT_STORAGE;
                    }
                    upload.CreateReply();
                    upload.Send();
                }else if (upload.type.Equals(Messages.TypeCode.RESPONSE)) {
                    if(upload.statuscode == StatusCode.ACCEPTED) {
                        IndexFile indexFile = _index.GetEntry(upload.filehash);
                        string filePath = indexFile.getPath();
                        FileSender fileSender = new FileSender(upload.from, upload.port);
                        fileSender.Send(filePath);
                    }
                }
            }
        }
    }
}