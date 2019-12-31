using System;
using System.Collections.Generic;

namespace JamesQMurphy.Auth
{
    public class ApplicationUser
    {
        private const string RECORD_TYPE_ID = "ID";
        private readonly Dictionary<string, ApplicationUserRecord> records = new Dictionary<string, ApplicationUserRecord>();

        public ApplicationUser()
        {
            // Create a single ApplicationUserRecord to represent the ID
            var miniGuid = NewMiniGuid();
            records.Add(RECORD_TYPE_ID, new ApplicationUserRecord(RECORD_TYPE_ID, miniGuid, miniGuid, miniGuid));
        }

        public ApplicationUser(IEnumerable<ApplicationUserRecord> applicationUserRecords)
        {
            foreach(var rec in applicationUserRecords)
            {
                records.Add(rec.Provider, rec);
            }
        }

        public string UserId => records[RECORD_TYPE_ID].Key;

        ////////////////////////////////////////////////////////


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
