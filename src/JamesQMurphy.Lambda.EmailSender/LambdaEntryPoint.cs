using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleSystemsManagement;
using JamesQMurphy.Email.Mailgun;
using System;

namespace JamesQMurphy.Lambda.EmailSender
{
    public static class LambdaEntryPoint
    {
        private static readonly string[] MessageSeparator = { Environment.NewLine };

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public static void Handler(SQSEvent sqsEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Received SQSEvent with {sqsEvent.Records.Count} SQSMessage record(s)");

            context.Logger.LogLine("Retrieving ApiKey");
            string apiKey = String.Empty;
            using (var ssmClient = new AmazonSimpleSystemsManagementClient())
            {
                var response = ssmClient.GetParameterAsync(
                    new Amazon.SimpleSystemsManagement.Model.GetParameterRequest
                    {
                        Name = Environment.GetEnvironmentVariable("Email__ServiceApiKeyName"),
                        WithDecryption = true
                    }
                ).GetAwaiter().GetResult();
                apiKey = response.Parameter.Value;
            }

            context.Logger.LogLine("Creating EmailService object");
            var emailService = new MailgunEmailService(new MailgunEmailService.Options
            {
                FromAddress = Environment.GetEnvironmentVariable("Email__FromAddress"),
                MailDomain = Environment.GetEnvironmentVariable("Email__MailDomain"),
                ServiceUrl = Environment.GetEnvironmentVariable("Email__ServiceUrl"),
                ServiceApiKey = apiKey
            });

            foreach (var sqsMessage in sqsEvent.Records)
            {
                context.Logger.LogLine($"Processing message {sqsMessage.MessageId}");

                // Parse out recipient, subject, and email body
                var strArray = sqsMessage.Body.Split(MessageSeparator, 3, StringSplitOptions.None);
                var recipient = strArray[0];
                context.Logger.LogLine($"({sqsMessage.MessageId}) To: {recipient}");
                var subject = strArray[1];
                context.Logger.LogLine($"({sqsMessage.MessageId}) Subject: {subject}");
                var body = strArray[2];
                context.Logger.LogLine($"({sqsMessage.MessageId}) Subject: {body}");

                // Send the e-mail
                var result = emailService.SendEmailAsync(recipient, subject, body).GetAwaiter().GetResult();
                context.Logger.LogLine($"({sqsMessage.MessageId}) Email service returned success: {result.Success} details: {result.Details}");
            }
        }
    }
}
