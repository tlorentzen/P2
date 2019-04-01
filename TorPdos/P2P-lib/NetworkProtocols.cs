using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.IO;
using System.Net;
using System.Net.Sockets;
using P2P_lib.Messages;
using Index_lib;
using P2P_lib;

namespace P2P_lib {
    public class NetworkProtocols {
        Receiver receiver;
        public void UploadFileToNetwork (string filePath, int seed, int copies, Network network) {
            List<Peer> peerlist = network.getPeerList();
            UploadMessage upload = new UploadMessage(peerlist[seed].GetIP());
            upload.filesize = new FileInfo(filePath).Length;
            upload.filename = new FileInfo(filePath).Name;
            upload.filehash = DiskHelper.CreateMD5(filePath);
            upload.path = filePath;
            upload.type = Messages.TypeCode.REQUEST;
            upload.statuscode = Messages.StatusCode.OK;
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
                }
            }
        }
    }
}
