using System;
using System.Threading.Tasks;

namespace JamesQMurphy.Email
{
    public class NullEmailService : IEmailService
    {
        public Task<EmailResult> SendEmailAsync(string emailAddress, string subject, string message)
        {
            return Task.FromResult(new EmailResult { Success = true, Details = "Null Email Service called" });
        }
    }
}
