using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JamesQMurphy.Web.Models;

namespace JamesQMurphy.Web.Services
{
    public enum EmailType
    {
        TestEmail = 0,
        EmailVerification,
        EmailAlreadyRegistered,
        PasswordReset
    }

    public interface IEmailGenerator
    {
        Task GenerateEmailAsync(ApplicationUser user, EmailType emailType, params string[] data);
    }
}
