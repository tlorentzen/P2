﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using P2P_lib.Messages;
using LogLevel = NLog.LogLevel;


namespace P2P_lib{
    public class Receiver{
        //This delegate can be used to point to methods
        //which return void and take a string.
        public delegate void DidReceive(BaseMessage msg);

        private static NLog.Logger logger = NLog.LogManager.GetLogger("ReceiverLogging");
        private static NLog.Logger slogger = NLog.LogManager.GetLogger("SocketException");


        //This event can cause any method which conforms
        //to MyEventHandler to be called.
        public event DidReceive MessageReceived;

        private IPAddress ip;
        private int port;
        private TcpListener server = null;
        private bool listening = false;
        private Thread listener;
        private byte[] _buffer = new byte[1024];


        public Receiver(int port){
            this.ip = IPAddress.Any;
            this.port = port;
        }

        public void start(){
            server = new TcpListener(this.ip, this.port);
            server.AllowNatTraversal(true);
            server.Start();


            listening = true;

            listener = new Thread(this.connectionHandler);
            listener.Start();
        }

        public void stop(){
            this.listening = false;
            server.Stop();
        }

        private void connectionHandler(){
            while (this.listening){
                try{
                    TcpClient client = server.AcceptTcpClient();
                    client.ReceiveTimeout = 500;
                    NetworkStream stream = client.GetStream();

                    int i;

                    using (MemoryStream memory = new MemoryStream()){
                        while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                            memory.Write(_buffer, 0, Math.Min(i, _buffer.Length));
                        }

                        memory.Seek(0, SeekOrigin.Begin);
                        byte[] messageBytes = new byte[memory.Length];
                        memory.Read(messageBytes, 0, messageBytes.Length);
                        memory.Close();

                        BaseMessage message = BaseMessage.FromByteArray(messageBytes);
                        MessageReceived(message);
                    }
                }
                catch (SocketException e){
                    slogger.Log(LogLevel.Info,e);
                }
                catch (Exception e){
                    logger.Log(LogLevel.Error, e);
                }
            }
        }
    }
}