using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JamesQMurphy.Email
{
    public class EmailResult
    {
        public bool Success { get; set; }
        public string Details { get; set; }
    }

    public interface IEmailService
    {
        Task<EmailResult> SendEmailAsync(string emailAddress, string subject, string message);
    }
}
