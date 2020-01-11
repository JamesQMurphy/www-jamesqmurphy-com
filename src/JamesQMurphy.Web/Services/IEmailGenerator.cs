using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public enum EmailType
    {
        TestEmail = 0,
        EmailVerification,
        EmailAlreadyRegistered,
        PasswordReset,
        PasswordChanged,
        Comments
    }

    public interface IEmailGenerator
    {
        Task GenerateEmailAsync(string emailAddress, EmailType emailType, params string[] data);
    }
}
