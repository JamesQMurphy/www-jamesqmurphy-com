using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JamesQMurphy.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string emailAddress, string subject, string message);
    }
}
