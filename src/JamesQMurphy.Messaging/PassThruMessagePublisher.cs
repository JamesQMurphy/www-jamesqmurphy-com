using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JamesQMurphy.Messaging
{
    public class PassThruMessagePublisher : IMessagePublisher
    {
        private readonly IMessageReceiver _messageReceiver;

        public PassThruMessagePublisher(IMessageReceiver messageReceiver)
        {
            _messageReceiver = messageReceiver;
        }

        public async Task PublishMessage(Message message)
        {
            await _messageReceiver.ReceiveMessage(message);
        }
    }
}
