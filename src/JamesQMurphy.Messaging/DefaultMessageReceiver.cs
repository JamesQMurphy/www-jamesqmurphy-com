using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamesQMurphy.Messaging
{
    public delegate void MessageReceivedDelegate(Message message, IServiceProvider serviceProvider);

    public class DefaultMessageReceiver : IMessageReceiver
    {
        private readonly Dictionary<MessageTypes, MessageReceivedEvent> _messageReceivedEvents = new Dictionary<MessageTypes, MessageReceivedEvent>();
        private readonly IServiceProvider _serviceProvider;

        public DefaultMessageReceiver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            foreach (var messageType in Enum.GetValues(typeof(MessageTypes)).Cast<MessageTypes>())
            {
                _messageReceivedEvents[messageType] = new MessageReceivedEvent();
            }
        }

        public void AddMessageHandler(MessageTypes messageType, MessageReceivedDelegate handler)
        {
            _messageReceivedEvents[messageType] += handler;
        }

        public Task ReceiveMessage(Message message)
        {
            // This is synchronous behavior, for now
            _messageReceivedEvents[message.MessageType].SendEvent(message, _serviceProvider);
            return Task.CompletedTask;
        }
    }

    // Based on https://stackoverflow.com/a/16135376/1001100
    internal class MessageReceivedEvent
    {
        private event MessageReceivedDelegate _eventDelegate;

        public static MessageReceivedEvent operator +(MessageReceivedEvent messageReceivedEvent, MessageReceivedDelegate eventDelegate)
        {
            messageReceivedEvent._eventDelegate += eventDelegate;
            return messageReceivedEvent;
        }

        public static MessageReceivedEvent operator -(MessageReceivedEvent messageReceivedEvent, MessageReceivedDelegate eventDelegate)
        {
            messageReceivedEvent._eventDelegate -= eventDelegate;
            return messageReceivedEvent;
        }

        public void SendEvent(Message message, IServiceProvider serviceProvider) => _eventDelegate?.Invoke(message, serviceProvider);
    }
}
