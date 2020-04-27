using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleSystemsManagement;
using JamesQMurphy.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JamesQMurphy.Messaging.Aws
{
    public static class LambdaEntryPoint
    {
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public static void Handler(SQSEvent sqsEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Received SQSEvent with {sqsEvent.Records.Count} SQSMessage record(s)");

            IConfiguration config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var serviceCollection = new ServiceCollection();
            new Startup(config).ConfigureServices(serviceCollection);
            var messageReceiver = new DefaultMessageReceiver(serviceCollection.BuildServiceProvider());

            foreach (var sqsMessage in sqsEvent.Records)
            {
                context.Logger.LogLine($"Processing message {sqsMessage.MessageId}");

                // TODO: deserialize message from sqsMessage
                var message = new Message(MessageTypes.None, "");
                messageReceiver.ReceiveMessage(message);
            }
        }
    }
}
