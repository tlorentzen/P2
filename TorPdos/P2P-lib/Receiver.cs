using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Index_lib;
using Microsoft.Win32;
using P2P_lib.Messages;

namespace P2P_lib{
    public class Receiver{
        RegistryKey MyReg = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");

        private HiddenFolder _hiddenFolder;

        //This delegate can be used to point to methods
        //which return void and take a string.
        public delegate void DidReceive(BaseMessage msg);

        //This event can cause any method which conforms
        //to MyEventHandler to be called.
        public event DidReceive MessageReceived;

        private IPAddress ip;
        private int port;
        private TcpListener server = null;
        private Boolean listening = false;
        private Thread listener;
        private byte[] buffer;

        public Receiver(int port, int bufferSize = 4000){
            this.ip = IPAddress.Any;
            this.port = port;
            this.buffer = new byte[bufferSize];
        }

        public void start(){
            server = new TcpListener(this.ip, this.port);
            //server.AllowNatTraversal(true);
            server.Start();

            listening = true;

            listener = new Thread(this.connectionHandler);
            listener.Start();
        }

        public void stop(){
            server.Stop();
            this.listening = false;
        }

        private void connectionHandler(){

            TcpClient client = null;
            NetworkStream stream = null;

            while (this.listening){
                try{
                    client = server.AcceptTcpClient();
                    stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(buffer, 0, buffer.Length)) != 0){
                        if (!this.listening){
                            break;
                        }
                    }

                    BaseMessage message = (BaseMessage) BaseMessage.FromByteArray(buffer);
                    MessageReceived(message);
                }
                catch (Exception e){
                    string path = MyReg.GetValue("Path").ToString();

                    _hiddenFolder = new HiddenFolder(path + @"\.hidden");
                    _hiddenFolder.AppendToFileLog(path + @"\.hidden\log.txt", DateTime.Now + e.ToString() + "\\n \\n --------------------------------------");

                }finally{
                    client.Close();
                    stream.Close();
                }
            }
        }
    }
}