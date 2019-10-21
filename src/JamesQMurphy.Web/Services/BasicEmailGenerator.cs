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
                        message = $@"
<html><body>
<p>Hello,
<br/></p>
<p>
Thank you for registering your e-mail address with Cold-Brewed DevOps!
<br/></p>
<p></br></p>
<p>
In order to complete the sign-up process, please click on the following link to confirm your e-mail address:<br/>
<br/>
<a href='{data[0]}'>{data[0]}</a><br/>
<br/>
This link will be valid for 48 hours.  If you do not verify your e-mail address before the link expires, you
will need to register again.
<br/></p>
<p></br></p>
<p>
Thank you,<br/>
<br/>
JamesQMurphy<br/>
</p>
</body></html>
";
                    }
                    break;

                case EmailType.EmailAlreadyRegistered:
                    subject = "Somebody tried to register your e-mail address";
                    message = $@"
<html><body>
<p>Hello,
<br/></p>
<p>
We thought you should know that somebody tried to register on Cold-Brewed DevOps using your e-mail address.
If this was you, then there's nothing to worry about it.  If you think it is somebody else, they still won't
be able to use your e-mail address, but it might be necessary to consider changing your password.
<br/></p>
<p></br></p>
<p>
Thank you,<br/>
<br/>
JamesQMurphy<br/>
</p>
</body></html>
";
                    break;
            }

            _ = await _emailService.SendEmailAsync(user.Email, subject, message);
        }
    }
}
