using System;

namespace JamesQMurphy.Web.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public string ApiRequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public bool ShowApiRequestId => !string.IsNullOrEmpty(ApiRequestId);
    }
}