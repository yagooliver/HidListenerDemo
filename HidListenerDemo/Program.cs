using System;
using System.Collections.Generic;
using System.Threading;
using HidListenerDemo.Model;
using HidListenerDemo.Service;
using HidListenerDemo.Service.Impl;

namespace HidListenerDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            IHidConnection hidConnection = new HidConnection();
            hidConnection.Messages = new Dictionary<Guid, Message>();

            SetCommands(hidConnection);
            
            Console.ReadKey();
        }

        private static void SetCommands(IHidConnection hidConnection)
        {
            bool isConnected = false;
            while (true)
            {
                Console.WriteLine("Set mac address");
                var macAddress = Console.ReadLine();

                Console.WriteLine("Select HID Command");
                Console.WriteLine("1: Set Time Zone");
                Console.WriteLine("2: Set Files");
                Console.WriteLine("3: Reset tasks");

                var value = Console.ReadLine();
                if (int.TryParse(value, out int selectedOption))
                {
                    if (!isConnected)
                    {
                        Thread thread = new Thread(() => hidConnection.StartConnection(4070));
                        thread.Start();
                        isConnected = true;
                    }

                    switch (selectedOption)
                    {
                        case 1:
                            Console.WriteLine("Setting TimeZone");
                            SetTimeZone(hidConnection, macAddress);
                            break;
                        case 2:
                            //TODO: To implement method to send Files
                            break;
                        case 3:
                            Console.WriteLine("Reseting Tasks");
                            ResetTasks(hidConnection, macAddress);
                            break;
                    }
                }
                Thread.Sleep(3000);

                Console.WriteLine("Do you want cancel the process? Y(yes) / N(no)");
                var option = Console.ReadLine();
                if (option != null && (!string.IsNullOrEmpty(option) && option.ToLower().Contains("y")))
                    break;
            }
        }

        private static void SetTimeZone(IHidConnection hidConnection, string mac)
        {
            Message message = new Message
            {
                Id = Guid.NewGuid(),
                Mac = mac,
                Command = hidConnection.FormatMessage("0088", "MST7MDT,M3.2.0/2,M11.1.0/2"),
                Executed = false,
                File = null
            };
            hidConnection.Messages.Add(message.Id, message);
            message = new Message
            {
                Id = Guid.NewGuid(),
                Mac = mac,
                Command = hidConnection.FormatMessage("0018", $"{DateTime.Now.Month};{DateTime.Now.Day};{DateTime.Now.Year};{DateTime.Now.Hour};{DateTime.Now.Minute};{DateTime.Now.Second}"),
                Executed = false,
                File = null
            };
            hidConnection.Messages.Add(message.Id, message);
        }

        private static void ResetTasks(IHidConnection hidConnection, string mac)
        {
            Message message = new Message
            {
                Id = Guid.NewGuid(),
                Mac = mac,
                Command = hidConnection.FormatMessage("0012", "1"),
                Executed = false,
                File = null
            };
            hidConnection.Messages.Add(message.Id, message);
            message = new Message
            {
                Id = Guid.NewGuid(),
                Mac = mac,
                Command = hidConnection.FormatMessage("0012", "2"),
                Executed = false,
                File = null
            };
            hidConnection.Messages.Add(message.Id, message);
            message = new Message
            {
                Id = Guid.NewGuid(),
                Mac = mac,
                Command = hidConnection.FormatMessage("0012", "3"),
                Executed = false,
                File = null
            };
            hidConnection.Messages.Add(message.Id, message);
            message = new Message
            {
                Id = Guid.NewGuid(),
                Mac = mac,
                Command = hidConnection.FormatMessage("0012", "4"),
                Executed = false,
                File = null
            };
            hidConnection.Messages.Add(message.Id, message);
            message = new Message
            {
                Id = Guid.NewGuid(),
                Mac = mac,
                Command = hidConnection.FormatMessage("0012", "5"),
                Executed = false,
                File = null
            };
            hidConnection.Messages.Add(message.Id, message);
        }
    }
}
