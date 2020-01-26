using JamesQMurphy.Auth;
using JamesQMurphy.Web.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class MockEmailGenerator : IEmailGenerator
    {
        // These are C# tuples (https://docs.microsoft.com/en-us/dotnet/csharp/tuples)
        private readonly List<(string emailAddress, EmailType emailType, string[] data)> _emails = new List<(string emailAddress, EmailType emailType, string[] data)>();

        public IList<(string emailAddress, EmailType emailType, string[] data)> Emails => _emails;

        public Task GenerateEmailAsync(string emailAddress, EmailType emailType, params string[] data)
        {
            _emails.Add((emailAddress, emailType, data));
            return Task.CompletedTask;
        }
    }
}
