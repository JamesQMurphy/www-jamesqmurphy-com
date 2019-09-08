using System;
using System.Threading.Tasks;

namespace JamesQMurphy.Email
{
    public class NullEmailService : IEmailService
    {
        public Task SendEmailAsync(string emailAddress, string subject, string message)
        {
            return Task.CompletedTask;
        }
    }
}
