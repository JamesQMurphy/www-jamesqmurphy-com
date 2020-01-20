using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Web.Models
{
    public enum AlertMessageTypes
    {
        Danger = 0,
        Warning,
        Success,
        Info,
        Primary,
        Secondary,
        Light,
        Dark
    }

    public class AlertMessage
    {
        public AlertMessageTypes AlertMessageType { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
    }
}
