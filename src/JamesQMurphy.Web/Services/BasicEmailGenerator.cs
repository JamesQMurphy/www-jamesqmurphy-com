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
        private readonly WebSiteOptions _webSiteOptions;

        public BasicEmailGenerator(IEmailService emailService, WebSiteOptions webSiteOptions)
        {
            _emailService = emailService;
            _webSiteOptions = webSiteOptions;
        }

        public async Task GenerateEmailAsync(string emailAddress, EmailType emailType, params string[] data)
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
Thank you for registering your e-mail address with {_webSiteOptions.WebSiteTitle}!
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
We thought you should know that somebody tried to register on {_webSiteOptions.WebSiteTitle} using your e-mail address.
If this was you, then there's nothing to worry about.  If you think it is somebody else, don't worry... that
person still cannot use your e-mail address.  But feel free to contact us if you have any questions.
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

                case EmailType.PasswordReset:
                    subject = "Reset Your Password";
                    if (data != null && data.Length > 0)
                    {
                        message = $@"
<html><body>
<p>Hello,<br/>
<br/></p>
<p></br></p>
<p>
Somebody (hopefully you!) requested to reset your password on {_webSiteOptions.WebSiteTitle}, and we want to
make sure it was really you.  To reset your password, click this link (or copy/paste it into your
browser) to be taken to the website, where you will be able to enter a new password:</br>
<br/>
<a href='{data[0]}'>{data[0]}</a><br/>
<br/>
If this wasn't you, or you've changed your mind, don't worry... we haven't done anything yet.  You 
can safely delete this message and nothing will happen.
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

                case EmailType.PasswordChanged:
                    subject = "Password changed";
                    message = $@"
<html><body>
<p>Hello,
<br/></p>
<p>
We are just letting you know that your password has been successfully changed on {_webSiteOptions.WebSiteTitle}.  If
this was you, then there's nothing to worry about.  But if you think that somebody else has changed
your password, please contact us immediately.
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


                case EmailType.Comments:
                    subject = $"Comments from {_webSiteOptions.WebSiteTitle} Contact Page";
                    message = $@"
Somebody has sent a comment from the {_webSiteOptions.WebSiteTitle} Contact Page:

Username: {(string.IsNullOrWhiteSpace(data[0]) ? "(not logged in)" : data[0])}
Comments:
{data[1]}
";
                    break;

            }

            _ = await _emailService.SendEmailAsync(emailAddress, subject, message);
        }
    }
}
