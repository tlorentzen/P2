﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using P2P_lib.Messages;
using LogLevel = NLog.LogLevel;

namespace P2P_lib{
    public class Receiver{

        /// <summary>
        /// This delegate can be used to point to methods without
        /// a return value and a BaseMessage (or subclass) as input.
        /// </summary>
        /// <param name="msg">The message to be handled.</param>
        public delegate void DidReceive(BaseMessage msg);

        private static NLog.Logger logger = NLog.LogManager.GetLogger("ReceiverLogging");
        private static NLog.Logger slogger = NLog.LogManager.GetLogger("SocketException");

        /// <summary>
        /// This event can cause any method which conforms
        /// to MyEventHandler to be called.
        /// </summary>
        public event DidReceive MessageReceived;

        private IPAddress ip;
        private int port;
        private TcpListener _server;
        private bool _listening;
        private Thread _listener;
        private byte[] _buffer = new byte[1024];

        public Receiver(int port){
            this.ip = IPAddress.Any;
            this.port = port;
        }

        /// <summary>
        /// Starts the functionality of the receiver
        /// by starting a TCPListener on a new thread.
        /// </summary>
        public void Start(){
            _server = new TcpListener(this.ip, this.port);
            _server.AllowNatTraversal(true);
            _server.Start();

            _listening = true;

            _listener = new Thread(this.connectionHandler);
            _listener.Start();
        }

        /// <summary>
        /// Stops the TCPListener.
        /// </summary>
        /// <returns>Rather the TCPListener has been stopped.</returns>
        public bool Stop(){
            this._listening = false;
            _server.Stop();
            return true;
        }

        /// <summary>
        /// A function to handle the TCPListener and receive
        /// packages from other devices.
        /// </summary>
        private async void connectionHandler(){
            while (this._listening){
                try{

                    while (!_server.Pending())
                    {
                        if(!this._listening)
                        {
                            break;
                        }
                    }
                    var client = await _server.AcceptTcpClientAsync();
                    client.ReceiveTimeout = 1000;
                    client.Client.ReceiveTimeout = 1000;

                    int i;
                    using (NetworkStream stream = client.GetStream()){
                        using (MemoryStream memory = new MemoryStream())
                        {
                            while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0)
                            {
                                memory.Write(_buffer, 0, Math.Min(i, _buffer.Length));
                            }
                            memory.Seek(0, SeekOrigin.Begin);
                            byte[] messageBytes = new byte[memory.Length];
                            memory.Read(messageBytes, 0, messageBytes.Length);
                            memory.Close();

                            BaseMessage message = BaseMessage.FromByteArray(messageBytes);
                            if (MessageReceived != null) MessageReceived.Invoke(message);
                        }
                    }
                    client.Close();
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