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
using Microsoft.Win32;
using P2P_lib.Messages;

namespace P2P_lib{
    public class Receiver{
        RegistryKey MyReg = Registry.CurrentUser.CreateSubKey("TorPdos\\1.1.1.1");
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
            server.Stop();
            this.listening = false;
        }

        private void connectionHandler(){

            while (this.listening){

                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    int i;

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

                        BaseMessage message = (BaseMessage)BaseMessage.FromByteArray(messageBytes);
                        MessageReceived(message);
                    }

                    

                   /*

                    while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0)
                    {
                        mem.Write(_buffer, );

                        fileStream.Write(_buffer, 0, (i < _buffer.Length) ? i : _buffer.Length);
                        //fileStream.Write(_buffer, 0, _buffer.Length);
                    }

                
                    //while(!stream.CanRead){
                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            

                            if (!this.listening)
                            {
                                break;
                            }
                        }
                    //}

                    */

                    
                }
                catch (Exception e){
                    
                    /*
                    string path = MyReg.GetValue("Path").ToString();
                    byte [] input = new byte[e.ToString().Length];
                    string error = DateTime.Now + input.ToString() + "\\n";
                    if (!File.Exists(path+"/.hidden/log.txt")){
                        File.Create(path + "/.hidden/log.txt");
                    }
                    input = Encoding.ASCII.GetBytes(error);
                    using (FileStream log = new FileStream(path+"/.hidden/log.txt",FileMode.Append)) {
                        log.Write(input,0,e.ToString().Length);
                        log.Close();
                    }
                    */
                }finally{
                    /*
                    if(client != null){
                        client.Close();
                    }
                    */
                    /*
                    if(stream != null){
                        stream.Close();
                    }
                    */
                }
            }
        }
    }
}