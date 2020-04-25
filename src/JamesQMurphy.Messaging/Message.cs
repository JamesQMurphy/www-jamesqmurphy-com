using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Messaging
{
    public sealed class Message
    {
        public readonly MessageTypes MessageType;
        public readonly string Data;

        public Message(MessageTypes messageType, string data)
        {
            MessageType = messageType;
            Data = data;
        }
    }
}
