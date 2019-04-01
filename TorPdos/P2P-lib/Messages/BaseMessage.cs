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
        public String ToUUID;
        public String FromUUID;
        public String to;
        public String from;
        private String hash;
        public StatusCode statuscode;
        public TypeCode type;

        public System.Type GetMessageType(){
            return this.GetType();
        }

        public abstract String GetHash();

        public BaseMessage(String to)
        {
            this.to = to;
        }

        public Boolean Send()
        {
            try{
                using (TcpClient client = new TcpClient(this.to, 25565))
                {
                    client.SendTimeout = 2000;

                    byte[] data = this.ToByteArray();
                    using (NetworkStream stream = client.GetStream())
                    {
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
        private byte[] ToByteArray()
        {
            if (this == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        public static BaseMessage FromByteArray(byte[] data)
        {
            if (data == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (BaseMessage)obj;
            }
        }
    }
}
