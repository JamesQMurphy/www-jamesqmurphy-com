using JamesQMurphy.Messaging;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace JamesQMurphy.Messaging.UnitTests
{
    public class DefaultMessageReceiverTests
    {
        private DefaultMessageReceiver _defaultMessageReceiver;

        [SetUp]
        public void Setup()
        {
            _defaultMessageReceiver = new DefaultMessageReceiver(null);
        }

        [Test]
        public void EventIsHandled()
        {
            var eventHandled = false;
            _defaultMessageReceiver.AddMessageHandler(
                MessageTypes.SendEmail,
                delegate (Message message, IServiceProvider serviceProvider)
                {
                    eventHandled = true;
                }
            );

            _defaultMessageReceiver
                .ReceiveMessage(new Message(MessageTypes.SendEmail, ""))
                .GetAwaiter()
                .GetResult();

            Assert.IsTrue(eventHandled);
        }

        [Test]
        public void EventHasData()
        {
            var eventHandled = false;
            var data = "some data";
            string dataReceived = "";

            _defaultMessageReceiver.AddMessageHandler(
                MessageTypes.SendEmail,
                delegate (Message message, IServiceProvider serviceProvider)
                {
                    eventHandled = true;
                    dataReceived = message.Data;
                }
            );

            _defaultMessageReceiver
                .ReceiveMessage(new Message(MessageTypes.SendEmail, data))
                .GetAwaiter()
                .GetResult();

            Assert.IsTrue(eventHandled);
            Assert.AreEqual(data, dataReceived);
        }

        [Test]
        public void OnlyHandleCorrectEvent()
        {
            var eventHandled = false;
            _defaultMessageReceiver.AddMessageHandler(
                MessageTypes.SendEmail,
                delegate (Message message, IServiceProvider serviceProvider)
                {
                    eventHandled = true;
                }
            );

            _defaultMessageReceiver
                .ReceiveMessage(new Message(MessageTypes.None, ""))
                .GetAwaiter()
                .GetResult();

            Assert.IsFalse(eventHandled);
        }


        class DummyService
        {
            public int Counter = 0;
        }

        [Test]
        public void ServiceGetsCalled()
        {
            var dummyService = new DummyService();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<DummyService>(dummyService);

            var defaultMessageReceiverWithServices = new DefaultMessageReceiver(serviceCollection.BuildServiceProvider());

            void handler(Message handlerMessage, IServiceProvider handlerServiceProvider)
            {
                var dummyServiceInHandler = handlerServiceProvider.GetService<DummyService>();
                dummyServiceInHandler.Counter = 1;
            }
            defaultMessageReceiverWithServices.AddMessageHandler(MessageTypes.SendEmail, handler);

            Assert.AreEqual(0, dummyService.Counter);
            defaultMessageReceiverWithServices.ReceiveMessage(new Message(MessageTypes.SendEmail, ""));
            Assert.AreEqual(1, dummyService.Counter);
        }
    }
}
