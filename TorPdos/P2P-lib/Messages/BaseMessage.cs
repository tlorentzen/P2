using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace P2P_lib.Messages
{
    public enum StatusCode { OK, ERROR, ACCEPTED, INSUFFICIENT_STORAGE };
    public enum TypeCode { REQUEST, RESPONSE };

    [Serializable]
    public abstract class BaseMessage
    {
        public string ToUUID;
        public string FromUUID;
        public string to;
        public string from;
        private string hash;
        public StatusCode statuscode;
        public TypeCode type;
        public int forwardCount;

        public System.Type GetMessageType(){
            return this.GetType();
        }

        public abstract string GetHash();

        public BaseMessage(string to){
            this.to = to;
        }

        public bool Send(int receiverPort = 25565){
            try{
                using (TcpClient client = new TcpClient(this.to, receiverPort)){
                    client.SendTimeout = 2000;

                    byte[] data = this.ToByteArray();
                    using (NetworkStream stream = client.GetStream()){
                        stream.Write(data, 0, data.Length);
                    }
                }

                return true;
            }catch(SocketException e){
                Console.WriteLine("SocketException");
                return false;
            }
        }

        //https://stackoverflow.com/questions/33022660/how-to-convert-byte-array-to-any-type
        private byte[] ToByteArray(){
            if (this == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()){
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        public static BaseMessage FromByteArray(byte[] data){
            if (data == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data)){
                object obj = bf.Deserialize(ms);
                return (BaseMessage)obj;
            }
        }

        public void CreateReply() {
            this.type = TypeCode.RESPONSE;
            string from_ip = this.from;
            this.from = this.to;
            this.to = from_ip;
        }

        public void forwardMessage(string toIP){
            this.to = toIP;
            this.type = TypeCode.REQUEST;
            forwardCount -= 1;
        }
    }
}
