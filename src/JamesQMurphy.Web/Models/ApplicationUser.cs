using System;

namespace JamesQMurphy.Web.Models
{
    public class ApplicationUser
    {
        public string NormalizedEmail { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public string NormalizedUserName { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; } = "";
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
    }
}
