using System;
using System.Collections.Generic;
using HidListenerDemo.Model;
using HidListenerDemo.Service.Impl;

namespace HidListenerDemo.Service
{
    public interface IHidConnection
    {
        Dictionary<Guid, Message> Messages { get; set; }

        void StartConnection(int port, string ipAddress = "");

        string ReceiveMessage();

        void SendMessage(string command);

        void SendCommand(string command, byte[] file);

        string FormatMessage(string code, string command);
    }
}
