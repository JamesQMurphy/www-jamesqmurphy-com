using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class ApplicationPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        public const int MIN_LENGTH = 6;

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
            var errors = new List<IdentityError>();
            if (string.IsNullOrWhiteSpace(password) || password.Length < MIN_LENGTH)
            {
                errors.Add(new IdentityError() { Description = $"Password must be at least {MIN_LENGTH} characters long" });
            }
            if (!password.Any(IsLower))
            {
                errors.Add(new IdentityError() { Description = "Password must have at least one lowercase letter" });
            }
            if (!password.Any(IsUpper))
            {
                errors.Add(new IdentityError() { Description = "Password must have at least one uppercase letter" });
            }
            if (!password.Any(IsNonLetter))
            {
                errors.Add(new IdentityError() { Description = "Password must have at least one number or symbol" });
            }
            return
                Task.FromResult(errors.Count == 0
                    ? IdentityResult.Success
                    : IdentityResult.Failed(errors.ToArray()));
        }

        public virtual bool IsLower(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        public virtual bool IsUpper(char c)
        {
            return c >= 'A' && c <= 'Z';
        }

        public virtual bool IsNonLetter(char c)
        {
            return !(IsUpper(c) || IsLower(c));
        }
    }
}
