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
        private readonly string _sourceAddress;
        public SESEmailService(string sourceAddress)
        {
            _sourceAddress = sourceAddress;
        }

        public async Task<EmailResult> SendEmailAsync(string emailAddress, string subject, string message)
        {
            using (var client = new AmazonSimpleEmailServiceClient())
            {
                var sendRequest = new SendEmailRequest()
                {
                    Source = _sourceAddress,
                    Destination = new Destination()
                    {
                        ToAddresses = new List<string> { emailAddress }
                    },
                    Message = new Message()
                    {
                        Subject = new Content(subject),
                        Body = new Body(new Content(message))
                    }
                };
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
