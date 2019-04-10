using System;
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

        public async void handler(){
            while (listening)
            {
                var client = await server.AcceptTcpClientAsync().ConfigureAwait(false);
                this.HandlerConnection(client);
                //var cw = new ClientWorking(client, true);
                //cw.DoSomethingWithClientAsync().NoWarning();
            }
        }

        public void start(){
            server = new TcpListener(this.ip, this.port);
            server.AllowNatTraversal(true);
            server.Start();
            _errorLoggerQueue = new ErrorLoggerQueue(_errorQueue, "Receiver");
            Thread thread = new Thread(_errorLoggerQueue.run);

            listening = true;
            handler();
        }

        public void stop(){
            server.Stop();
            this.listening = false;
        }

        private async void connectionHandler(){

            while (this.listening){

                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync();

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

        private async void HandlerConnection(TcpClient client)
        {
            try
            {
                using (var stream = client.GetStream())
                {
                    int i;

                    using (MemoryStream memory = new MemoryStream())
                    {
                        while ((i = await stream.ReadAsync(_buffer, 0, _buffer.Length)) > 0)
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
                }
            }
            catch (Exception e)
            {
                string path = MyReg.GetValue("Path").ToString();

                _hiddenFolder = new HiddenFolder(path + @"\.hidden");
                _hiddenFolder.AppendToFileLog(path + @"\.hidden\log.txt", e.ToString());

            }
        }
    }
}