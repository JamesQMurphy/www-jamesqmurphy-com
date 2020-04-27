using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Email
{
    public class EmailMessage
    {
        public string EmailAddress { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
    }
}
