using System;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace P2P_lib.Messages{
    public enum StatusCode{
        OK, ERROR, ACCEPTED,
        INSUFFICIENT_STORAGE, FILE_NOT_FOUND
    };

    public enum TypeCode{ REQUEST, RESPONSE };


    [Serializable]
    public abstract class BaseMessage{
        private string toUuid;
        public string fromUuid;
        public string to;
        public string from;
        public StatusCode statusCode;
        public TypeCode type;
        private readonly NLog.Logger _logger = NLog.LogManager.GetLogger("NetworkLogging");

        public Type GetMessageType(){
            return this.GetType();
        }

        public abstract string GetHash();

        public BaseMessage(Peer to){
            this.toUuid = to.GetUuid();
            this.to = to.GetIp();
            this.fromUuid = IdHandler.GetUuid();
            this.from = NetworkHelper.GetLocalIpAddress();
            this.type = TypeCode.REQUEST;
            this.statusCode = StatusCode.OK;
        }

        public bool Send(int receiverPort = 25565){
            try{
                var connectionTester = new TcpClient();
                var result = connectionTester.BeginConnect(this.to, receiverPort, null, null);

                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                if (success){
                    try{
                        byte[] data = this.ToByteArray();

                        using (NetworkStream stream = connectionTester.GetStream()){
                            stream.Write(data, 0, data.Length);
                            stream.Close();
                        }
                    }
                    catch (Exception e){
                        _logger.Fatal(e);
                    }

                    connectionTester.Close();
                } else{
                    _logger.Info(new TimeoutException());
                }

                return true;
            }
            catch (SocketException e){
                _logger.Info(e);
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
                ms.Close();
                return (BaseMessage) obj;
            }
        }

        public void CreateReply(){
            this.type = TypeCode.RESPONSE;
            string fromIp = this.from;
            this.from = this.to;
            this.to = fromIp;
            string inputFromUuid = this.toUuid;
            this.toUuid = this.fromUuid;
            this.fromUuid = inputFromUuid;
        }

        public void ForwardMessage(string toIp){
            this.to = toIp;
            this.type = TypeCode.REQUEST;
        }
    }
}