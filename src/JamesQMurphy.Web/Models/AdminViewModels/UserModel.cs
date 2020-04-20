using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Models.AdminViewModels
{
    public class UserModel
    {
        public string userName { get; set; }
        public string userId { get; set; }
        public string email { get; set; }
        public bool emailVerified { get; set; }
        public IEnumerable<(string,string)> externalLogins { get; set; }
        public DateTime lastUpdatedUtc { get; set; }
    }
}
