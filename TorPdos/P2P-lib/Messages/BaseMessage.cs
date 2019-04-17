﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using NLog.Fluent;

namespace P2P_lib.Messages{
    public enum StatusCode{
        OK, ERROR, ACCEPTED,
        INSUFFICIENT_STORAGE, FILE_NOT_FOUND
    };

    public enum TypeCode{ REQUEST, RESPONSE };


    [Serializable]
    public abstract class BaseMessage{
        public string ToUUID;
        public string FromUUID;
        public string to;
        public string from;
        private string hash;
        public StatusCode statuscode;
        public TypeCode type;
        public int forwardCount;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("NetworkLogging");

        public System.Type GetMessageType(){
            return this.GetType();
        }

        public abstract string GetHash();

        public BaseMessage(Peer to){
            this.ToUUID = to.getUUID();
            this.to = to.GetIP();
            this.FromUUID = DiskHelper.getRegistryValue("UUID");
            this.from = NetworkHelper.getLocalIPAddress();
            this.type = TypeCode.REQUEST;
            this.statuscode = StatusCode.OK;
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
                        logger.Fatal(e);
                    }

                    connectionTester.EndConnect(result);
                    connectionTester.Close();
                } else{
                    logger.Fatal(new TimeoutException());
                }

                return true;
            }
            catch (SocketException e){
                logger.Info(e);
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
            string _fromIP = this.from;
            this.from = this.to;
            this.to = _fromIP;
            string _fromUUID = this.ToUUID;
            this.ToUUID = this.FromUUID;
            this.FromUUID = _fromUUID;
        }

        public void forwardMessage(string toIP){
            this.to = toIP;
            this.type = TypeCode.REQUEST;
            forwardCount -= 1;
        }
    }
}