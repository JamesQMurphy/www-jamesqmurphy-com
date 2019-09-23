using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class ApplicationPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        public const string REGEX_PATTERN = @"^(?:(?=.*[a-z])(?:(?=.*[A-Z])(?=.*[\d\W]))).{6,}$";
        public const string REGEX_DESCRIPTION = "Password must be at least six characters long and contain at least one lowercase letter, one uppercase letter, and a number or symbol.";

        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (Regex.IsMatch(password, REGEX_PATTERN, RegexOptions.CultureInvariant))
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError[]
                {
                    new IdentityError { Description = REGEX_DESCRIPTION}
                }));
            }
        }
    }
}
