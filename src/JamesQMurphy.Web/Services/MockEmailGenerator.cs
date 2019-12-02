using JamesQMurphy.Auth;
using JamesQMurphy.Web.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class MockEmailGenerator : IEmailGenerator
    {
        // These are C# tuples (https://docs.microsoft.com/en-us/dotnet/csharp/tuples)
        private readonly List<(ApplicationUser user, EmailType emailType, string[] data)> _emails = new List<(ApplicationUser user, EmailType emailType, string[] data)>();

        public IList<(ApplicationUser user, EmailType emailType, string[] data)> Emails => _emails;

        public Task GenerateEmailAsync(ApplicationUser user, EmailType emailType, params string[] data)
        {
            _emails.Add((user, emailType, data));
            return Task.CompletedTask;
        }
    }
}
