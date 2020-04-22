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

        public async Task<EmailResult> SendEmailAsync(EmailMessage emailMessage)
        {
            using (var client = new AmazonSimpleEmailServiceClient())
            {
                var sendRequest = new SendEmailRequest()
                {
                    Source = _options.FromAddress,
                    Destination = new Destination()
                    {
                        ToAddresses = new List<string> { emailMessage.EmailAddress }
                    },
                    Message = new Message()
                    {
                        Subject = new Content(emailMessage.Subject),
                        Body = new Body()
                    }
                };
                if (emailMessage.Body.Trim().ToLowerInvariant().StartsWith("<html>"))
                {
                    sendRequest.Message.Body.Html = new Content(emailMessage.Body);
                }
                else
                {
                    sendRequest.Message.Body.Text = new Content(emailMessage.Body);
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
