using System;

namespace JamesQMurphy.Auth
{
    public class ApplicationUser
    {
        public string UserId { get; set; } = NewMiniGuid();
        public string NormalizedEmail { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public string NormalizedUserName { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; } = "";
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
        public bool IsAdministrator { get; set; } = false;

        private static string NewMiniGuid()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", String.Empty);
        }
    }
}
