using System;

namespace HidListenerDemo.Model
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Mac { get; set; }
        public string Command { get; set; }
        public string Answer { get; set; }
        public bool Executed { get; set; }
        public byte[] File;
    }
}
