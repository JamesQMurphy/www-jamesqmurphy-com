using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JamesQMurphy.Messaging
{
    public interface IMessageReceiver
    {
        Task ReceiveMessage(Message message);
    }
}
