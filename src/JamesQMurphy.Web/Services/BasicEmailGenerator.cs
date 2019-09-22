using JamesQMurphy.Email;
using JamesQMurphy.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class BasicEmailGenerator : IEmailGenerator
    {
        private readonly IEmailService _emailService;

        public BasicEmailGenerator(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task GenerateEmailAsync(ApplicationUser user, EmailType emailType, params string[] data)
        {
            var subject = "No subject defined for " + emailType.ToString();
            var message = "No message defined for " + emailType.ToString();

            switch (emailType)
            {
                case EmailType.TestEmail:
                    subject = "Test e-mail message";
                    if (data != null && data.Length > 0)
                    {
                        message = $"Test message:{Environment.NewLine}{data[0]}";
                    }
                    break;

                case EmailType.EmailVerification:
                    subject = "Please verify your e-mail address";
                    if (data != null && data.Length > 0)
                    {
                        message = $"Please click on the following link to confirm your e-mail:{Environment.NewLine}<a href='{data[0]}'>{data[0]}</a>";
                    }
                    break;

                case EmailType.EmailAlreadyRegistered:
                    subject = "Somebody tried to register your e-mail address";
                    message = "Somebody tried to register your e-mail address.  If this was you, you may need to reset your password.";
                    break;
            }

            _ = await _emailService.SendEmailAsync(user.Email, subject, message);
        }
    }
}
