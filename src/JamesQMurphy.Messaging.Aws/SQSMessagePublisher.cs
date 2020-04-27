using Amazon.SQS;
using Amazon.SQS.Model;
using JamesQMurphy.Messaging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace JamesQMurphy.Messaging.Aws
{
    public class SQSMessagePublisher : IMessagePublisher
    {
        public class Options
        {
            public string QueueUrl { get; set; }
        }

        private readonly Options _options;
        private readonly AmazonSQSClient _sqsClient;
        public SQSMessagePublisher(Options options)
        {
            _options = options;

            _sqsClient = new AmazonSQSClient(new AmazonSQSConfig()
            {
                ServiceURL = "http://sqs.us-east-1.amazonaws.com"
            });

        }

        public async Task PublishMessage(Message message)
        {
            var sendMessageRequest = new SendMessageRequest(_options.QueueUrl, JsonSerializer.Serialize(message));
            _ = await _sqsClient.SendMessageAsync(sendMessageRequest);
        }
    }
}
