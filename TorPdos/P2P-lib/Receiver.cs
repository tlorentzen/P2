﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ErrorLogger;
using P2P_lib.Messages;


namespace P2P_lib{
    public class Receiver{
        //This delegate can be used to point to methods
        //which return void and take a string.
        public delegate void DidReceive(BaseMessage msg);

        private ErrorQueueHandler<string> _errorQueue = new ErrorQueueHandler<string>();
        private ErrorLoggerQueue _errorLoggerQueue;

        //This event can cause any method which conforms
        //to MyEventHandler to be called.
        public event DidReceive MessageReceived;

        private IPAddress ip;
        private int port;
        private TcpListener server = null;
        private Boolean listening = false;
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
            _errorLoggerQueue = new ErrorLoggerQueue(_errorQueue, "Receiver");
            Thread thread = new Thread(_errorLoggerQueue.run);
            thread.Start();

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

                        BaseMessage message = (BaseMessage) BaseMessage.FromByteArray(messageBytes);
                        MessageReceived(message);
                    }
                }
                catch (Exception e){
                    _errorQueue.Enqueue(e.ToString());
                    
                }
            }
        }
    }
}