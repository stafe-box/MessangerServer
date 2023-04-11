using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Media;

namespace MessangerServer
{
    internal class Messanger
    {
        bool _isActive = false;
        IPEndPoint? _lastGotIP;

        public delegate void Users(string text, Color color);
        public event Users? UserState;
        public delegate void GotException(string e);
        public event GotException? Error;
        private void Listen()
        {
            TcpListener? server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13255;
                IPAddress localAddr = IPAddress.Any;

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[512];

                // Enter the listening loop.
                while (_isActive)
                {
                    String? data = null;
                    using TcpClient client = server.AcceptTcpClient();
                    
                    data = null;

                    _lastGotIP = client.Client.RemoteEndPoint as IPEndPoint;
                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();
                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                    }
                    if (data != null)
                    {
                        Task.Run(() =>
                        {
                            this.MessagesWorker(data);
                        });
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e}");//событие с ошибкой
            }
            finally
            {
                server?.Stop();
            }
        }

        private void Send(string server, string message)
        {
            try
            {
                Int32 port = 13255;
                // Prefer a using declaration to ensure the instance is Disposed later.
                using TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

                // Get a client stream for reading and writing.
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                this.MessagesWorker(responseData);
            }
            catch (ArgumentNullException e)
            {
                _isActive = false;
                //SendExp?.Invoke($"ArgumentNullException: {e}");
            }
            catch (SocketException e)
            {
                //_isActive = false;
                this.RemoveUser(message);
            }
        }

        private void RemoveUser(string message)
        {
            Message msg = Message.Deserialize(message);
            if (msg != null)
            {
                Info.Users.Remove(msg.Recepient);
                UserState?.Invoke($"User {msg.Recepient} removed", Color.FromRgb(255, 10, 10));
                Message nMsg = new Message(
                    true,
                    msg.Recepient,
                    "@all",
                    "UserLeft",
                    Color.FromRgb(255, 10, 10));
                string json = nMsg.Serialize();
                foreach (var item in Info.Users)
                {
                    if (item.Key == "@all") { }
                    else
                    {
                        Send(item.Value, json);
                    }
                }
            }
        }

        public void Start()
        {
            _isActive = true;
            Task.Run(() => Listen());
        }
        public void Stop() { _isActive = false;}

        private void MessagesWorker(string json)
        {
            try
            {
                Message msg = Message.Deserialize(json);
                if (msg.IsService)
                {
                    switch (msg.Text) 
                    {
                        case "Register":
                            Message newMsg;
                            if (Info.Users.ContainsKey(msg.Sender))
                            {
                                newMsg = new Message
                                    (true,
                                    "Server",
                                    msg.Sender,
                                    "Denied",
                                    msg.Color);
                            }
                            else
                            {
                                newMsg = new Message
                                    (true,
                                    "Server",
                                    msg.Sender,
                                    "Registered",
                                    msg.Color);
                                Info.Users.Add(msg.Sender, _lastGotIP.Address.ToString());
                                UserState?.Invoke($"New User : {msg.Sender}", msg.Color);
                                Message nMsg = new Message(
                                                    true,
                                                    msg.Recepient,
                                                    "@all",
                                                    "UserLeft",
                                                    msg.Color);
                                string njson = nMsg.Serialize();
                                foreach (var item in Info.Users)
                                {
                                    if (item.Key == "@all") { }
                                    else
                                    {
                                        Send(item.Value, njson);
                                    }
                                }
                            }
                            this.Send(_lastGotIP.Address.ToString(), newMsg.Serialize());
                            break;
                        case "Alive":
                            break;
                    }
                }
                else
                {
                    if (msg.Recepient == "@all")
                    {
                        foreach (var item in Info.Users)
                        {
                            if (item.Key == "@all") { }
                            else
                            {
                                Send(item.Value, json);
                            }
                        }
                    }
                    else
                    {
                        Send(msg.Recepient, json);
                    }
                }
            }
            catch (Exception e) { }
        }
        private void Survey() 
        {
            Task.Run(async () =>
            {
                while (_isActive)
                {
                    foreach (var item in Info.Users)
                    {
                        if (item.Key == "@all") { }
                        else
                        {
                            Message msg = new Message(true,
                                "Server",
                                item.Key,
                                "IsAlive",
                                Color.FromRgb(0, 0, 0));
                            Send(item.Value, msg.Serialize());
                        }
                    }
                    await Task.Delay(2000);
                }
            });
        }
    }
}
