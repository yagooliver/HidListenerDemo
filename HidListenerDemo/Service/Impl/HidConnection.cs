using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HidListenerDemo.Model;

namespace HidListenerDemo.Service.Impl
{
    public class HidConnection : IHidConnection
    {
        private string _mac;
        private TcpListener _tcpListener;
        private TcpClient _tcpClient;
        private Guid _commandId;
        private bool _isConnected;
        public Dictionary<Guid, Message> Messages { get; set; }

        public string ReceiveMessage()
        {
            NetworkStream networkStream = _tcpClient.GetStream();
            networkStream.ReadTimeout = 500;
            byte[] buffer = new byte[4096];
            StringBuilder messageBuilder = new StringBuilder();
            do
            {
                int length = 0;
                try
                {
                    length = networkStream.Read(buffer, 0, buffer.Length);
                }
                catch (Exception)
                {
                    // ignored
                }
                messageBuilder.AppendFormat($"{Encoding.ASCII.GetString(buffer, 0, length)}");
            } while (networkStream.DataAvailable);
            return messageBuilder.ToString();
        }

        public void SendMessage(string command)
        {
            try
            {
                //Console.WriteLine($"sending: {command}");
                var networkStream = _tcpClient.GetStream();
                networkStream.Write(Encoding.ASCII.GetBytes(command), 0, Encoding.ASCII.GetBytes(command).Length);

                networkStream.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void SendCommand(string command, byte[] file)
        {
            try
            {
                var networkStream = _tcpClient.GetStream();
                if (command.Substring(0, 4) == "0032" || command.Substring(0, 4) == "0031")
                {
                    var fileBytes = file ?? File.ReadAllBytes(command.Split(';')[2]);
                    var nullFileBytes = new byte[fileBytes.Length];

                    for (var i = 0; i < fileBytes.Length; i++)
                    {
                        nullFileBytes[i] = fileBytes[i];
                    }

                    var action = $"{command.Split(';')[0]};{(fileBytes.Length + 10):D4};";
                    var asciiCommad = Encoding.ASCII.GetBytes($"{action}");
                    var bytefinal = new byte[asciiCommad.Length + nullFileBytes.Length];
                    Buffer.BlockCopy(asciiCommad, 0, bytefinal, 0, asciiCommad.Length);
                    Buffer.BlockCopy(nullFileBytes, 0, bytefinal, asciiCommad.Length, nullFileBytes.Length);

                    //Console.WriteLine($"Sending: {action}");
                    networkStream.Write(bytefinal, 0, bytefinal.Length);
                }
                else
                {
                    //Console.WriteLine($"Sending: {command}");
                    networkStream.Write(Encoding.ASCII.GetBytes(command), 0, Encoding.ASCII.GetBytes(command).Length);
                }

                networkStream.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void StartConnection(int port, string ipAddress = "")
        {
            try
            {
                if (port == 0) return;

                if (!_isConnected)
                {
                    _tcpListener = new TcpListener(IPAddress.Any, port);
                    _tcpListener.Start();
                    _isConnected = true;
                }
                Console.WriteLine("TCP connection was initialize for Hid controllers, waiting connections");
                while (true)
                {
                    _tcpClient = _tcpListener.AcceptTcpClient();
                    Console.WriteLine($"Connection accepted: {_tcpClient.Client.Handle}");
                    Console.WriteLine($"Controller connection in Handle: {_tcpClient.Client.Handle}");
                    Thread thread = new Thread(TcpConnection);
                    thread.Start();
                }
            }
            catch (Exception e)
            {
                _tcpListener.Server.Dispose();
                _isConnected = false;
                Console.WriteLine(e.Message);
                Console.WriteLine("Finish all connections");
                StartConnection(port, ipAddress);
            }
        }

        public void TcpConnection()
        {
            while (true)
            {
                try
                {
                    if (_tcpClient.Connected)
                    {
                        var result = ReceiveMessage();
                        if (result.Trim().Length == 0)
                        {
                            if(_mac.Equals(""))
                                continue;

                            var message = Messages.FirstOrDefault(m => m.Value.Mac.Contains(_mac) && !m.Value.Executed).Value;

                            if(message == null)
                                continue;

                            _commandId = message.Id;
                            SendCommand(message.Command, message.File);
                            message.Executed = true;
                        }
                        else
                        {
                            switch (result.Substring(0, 4))
                            {
                                case "1042":
                                    _mac = result.Substring(10, 17);
                                    result = FormatMessage("0070", "");
                                    SendMessage(result);
                                    break;
                                case "1080":
                                    result = FormatMessage("0080", "");
                                    SendMessage(result);
                                    break;
                                case "1065":
                                    break;
                                default:
                                    Console.WriteLine($"****************MAC: {_mac} \n ****************Answer: {result}");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        //_tcpClient.Client.Close();
                        if (_commandId != Guid.Empty && Messages?.Count > 0)
                        {
                            if (Messages != null && Messages[_commandId].Executed)
                            {
                                Messages?.Remove(_commandId);
                                _commandId = Guid.Empty;
                            }
                        }
                        //Console.WriteLine("Connection failed");
                        break;
                    }
                }
                catch (Exception e)
                {
                    if (_commandId != Guid.Empty && Messages?.Count > 0)
                    {
                        if (Messages!= null && Messages[_commandId].Executed)
                        {
                            Messages?.Remove(_commandId);
                            _commandId = Guid.Empty;
                        }
                    }

                    //Console.WriteLine("Connection finished");
                    //Console.WriteLine($"Error: {e.Message}");
                    break;
                }
            }
        }

        public string FormatMessage(string code, string command)
        {
            if (command.Equals(""))
            {
                int length = code.Length + 6 + 2;
                return $@"{code};{length:D4};0;";
            }
            else
            {
                int length = code.Length + command.Length + 7 + 2;
                return $@"{code};{length:D4};{command};0;";
            }
        }
    }
}
