using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Blog
{
    public class AlertMessage
    {
        public AlertMessageTypes AlertMessageType { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
    }
}
