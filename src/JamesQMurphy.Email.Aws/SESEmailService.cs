using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using JamesQMurphy.Email;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JamesQMurphy.Email.Aws
{
    public class SESEmailService : IEmailService
    {
        public class Options
        {
            public string FromAddress { get; set; }
        }

        private readonly Options _options;
        public SESEmailService(Options options)
        {
            _options = options;
        }

        public async Task<EmailResult> SendEmailAsync(string emailAddress, string subject, string message)
        {
            using (var client = new AmazonSimpleEmailServiceClient())
            {
                var sendRequest = new SendEmailRequest()
                {
                    Source = _options.FromAddress,
                    Destination = new Destination()
                    {
                        ToAddresses = new List<string> { emailAddress }
                    },
                    Message = new Message()
                    {
                        Subject = new Content(subject),
                        Body = new Body()
                    }
                };
                if (message.Trim().ToLowerInvariant().StartsWith("<html>"))
                {
                    sendRequest.Message.Body.Html = new Content(message);
                }
                else
                {
                    sendRequest.Message.Body.Text = new Content(message);
                }
                var response = await client.SendEmailAsync(sendRequest);
                return new EmailResult
                {
                    Success = (response.HttpStatusCode == System.Net.HttpStatusCode.OK),
                    Details = $"SendEmailAsync returned {response.HttpStatusCode}; message ID {response.MessageId}"
                };
            }
        }
    }
}
