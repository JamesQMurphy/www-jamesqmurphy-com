using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models.AdminViewModels
{
    public class ContactUsReaction
    {
        public string commentId { get; set; }
        public string userName { get; set; }
        public string userId { get; set; }
        public string timestamp { get; set; }
        public string htmlContent { get; set; }
    }
}
