using JamesQMurphy.Auth;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public enum EmailType
    {
        TestEmail = 0,
        EmailVerification,
        EmailAlreadyRegistered,
        PasswordReset,
        PasswordChanged
    }

    public interface IEmailGenerator
    {
        Task GenerateEmailAsync(ApplicationUser user, EmailType emailType, params string[] data);
    }
}
