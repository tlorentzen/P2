using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using P2P_lib.Messages;

namespace P2P_lib{
    public class Listener{
        private TcpListener _listener;
        private int _buffer_size;

        public Listener(int port) : this(port, 1024){ }

        public Listener(int port, int bufferSize){
            _buffer_size = bufferSize;

            try{
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.AllowNatTraversal(true);
            }
            catch (Exception e){
                //Logger.Error(e);
            }
        }

        /// <summary>
        /// Sends a message to an ip, as well as a port.
        /// The message can be of any type inheriting from the BaseMessage type.
        /// The function then checks whether the responding message is of the same type.
        /// </summary>
        /// <param name="msg">The message for which to be sent, must inherit from BaseMessage. This is a reference, so it will be updated</param>
        /// <param name="timeout">The milliseconds of timeout to wait before timing out if no message is received.</param>
        /// <typeparam name="T"> The message class object. Must inherit from BaseMessage</typeparam>
        /// <returns>Returns a bool of whether the message has been received.</returns>
        public bool SendAndAwaitResponse<T>(ref T msg, int timeout) where T : BaseMessage{
            try{
                bool success = true;
                int timeout_counter = 0;
                _listener.Start();
                msg.Send();

                while (!_listener.Pending()){
                    if (timeout_counter >= timeout){
                        _listener.Stop();
                        msg = null;
                        return false;
                    }

                    timeout_counter++;
                    System.Threading.Thread.Sleep(5);
                }

                var client = _listener.AcceptTcpClient();
                client.ReceiveTimeout = timeout;

                byte[] _buffer = new byte[this._buffer_size];

                using (NetworkStream stream = client.GetStream()){
                    int i;
                    using (MemoryStream memory = new MemoryStream()){
                        while ((i = stream.Read(_buffer, 0, _buffer.Length)) > 0){
                            memory.Write(_buffer, 0, Math.Min(i, _buffer.Length));
                        }

                        memory.Seek(0, SeekOrigin.Begin);
                        byte[] messageBytes = new byte[memory.Length];
                        memory.Read(messageBytes, 0, messageBytes.Length);
                        memory.Close();

                        var baseMsg = BaseMessage.FromByteArray(messageBytes);

                        if (baseMsg.GetMessageType() == typeof(T) && baseMsg.type == Messages.TypeCode.RESPONSE){
                            msg = (T) baseMsg;
                            success = true;
                        } else{
                            success = false;
                        }
                    }
                }

                client.Close();
                _listener.Stop();
                return success;
            }
            catch (SocketException e){
                return false;
            }
        }
    }
}