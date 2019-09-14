using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace JamesQMurphy.Web.Services
{
    public class InMemoryUserStore :
        IDisposable,
        IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>
    {
        private readonly Dictionary<string, ApplicationUser> _dictUsers = new Dictionary<string, ApplicationUser>();

        public InMemoryUserStore()
        {
            var pwhasher = new PasswordHasher<ApplicationUser>();

            var user = new ApplicationUser()
            {
                Email = "a@b",
                EmailConfirmed = true,
                NormalizedEmail = "A@B",
                NormalizedUserName = "USERNAME",
                UserName = "Username"
            };

            _ = CreateAsync(user).GetAwaiter().GetResult();
            _ = SetPasswordHashAsync(user, pwhasher.HashPassword(user, "abcde"));

        }

        #region Helpers
        public Task<string> GetUserIdAsync(ApplicationUser user) => Task.FromResult(user.NormalizedEmail);
        public Task<string> GetUserNameAsync(ApplicationUser user) => Task.FromResult(user.UserName);
        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user) => Task.FromResult(user.NormalizedUserName);
        public Task<string> GetPasswordHashAsync(ApplicationUser user) => Task.FromResult(user.PasswordHash);
        public Task<string> GetEmailAsync(ApplicationUser user) => Task.FromResult(user.Email);
        public Task<string> GetNormalizedEmailAsync(ApplicationUser user) => Task.FromResult(user.NormalizedEmail);
        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user) => Task.FromResult(user.EmailConfirmed);



        public Task SetUserNameAsync(ApplicationUser user, string userName)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedUserName)
        {
            user.NormalizedUserName = normalizedUserName;
            return Task.FromResult(0);
        }
        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }
        public Task SetEmailAsync(ApplicationUser user, string email)
        {
            user.Email = email;
            return Task.FromResult(0);
        }
        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }
        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }


        public Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            if (_dictUsers.TryAdd(user.NormalizedEmail, user))
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError() { Description = "Already present" }));
            }
        }
        public Task<IdentityResult> UpdateAsync(ApplicationUser user)
        {
            _dictUsers[user.NormalizedEmail] = user;
            return Task.FromResult(IdentityResult.Success);
        }
        public Task<IdentityResult> DeleteAsync(ApplicationUser user)
        {
            if (_dictUsers.Remove(user.NormalizedEmail))
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError() { Description = "User not found" }));
            }
        }
        public Task<ApplicationUser> FindByPrimaryKey(string primaryKey)
        {
            if (_dictUsers.ContainsKey(primaryKey))
            {
                return Task.FromResult(_dictUsers[primaryKey]);
            }
            else
            {
                return Task.FromResult((ApplicationUser)null);
            }
        }
        #endregion

        #region IDisposable Implementation
        private bool disposedValue = false; // To detect redundant calls
        public void Dispose()
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }
        #endregion


        #region IUserStore<ApplicationUser> implementation
        Task<IdentityResult> IUserStore<ApplicationUser>.CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return CreateAsync(user);
        }

        Task<IdentityResult> IUserStore<ApplicationUser>.DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return DeleteAsync(user);
        }

        Task<ApplicationUser> IUserStore<ApplicationUser>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return FindByPrimaryKey(userId);
        }

        Task<ApplicationUser> IUserStore<ApplicationUser>.FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = _dictUsers.Values.Where(u => u.NormalizedUserName == normalizedUserName).FirstOrDefault();
            return Task.FromResult(user);
        }

        Task<string> IUserStore<ApplicationUser>.GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetNormalizedUserNameAsync(user);
        }

        Task<string> IUserStore<ApplicationUser>.GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetUserIdAsync(user);
        }

        Task<string> IUserStore<ApplicationUser>.GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetUserNameAsync(user);
        }

        Task IUserStore<ApplicationUser>.SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SetNormalizedUserNameAsync(user, normalizedName);
        }

        Task IUserStore<ApplicationUser>.SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SetUserNameAsync(user, userName);
        }

        Task<IdentityResult> IUserStore<ApplicationUser>.UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return UpdateAsync(user);
        }
        #endregion


        #region IUserPasswordStore<ApplicationUser> implementation
        Task IUserPasswordStore<ApplicationUser>.SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SetPasswordHashAsync(user, passwordHash);
        }

        Task<string> IUserPasswordStore<ApplicationUser>.GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetPasswordHashAsync(user);
        }

        Task<bool> IUserPasswordStore<ApplicationUser>.HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(!String.IsNullOrEmpty(user.PasswordHash));
        }

        #endregion


        #region IUserEmailStore<ApplicationUser> implementation
        Task IUserEmailStore<ApplicationUser>.SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SetEmailAsync(user, email);
        }

        Task<string> IUserEmailStore<ApplicationUser>.GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetEmailAsync(user);
        }

        Task<bool> IUserEmailStore<ApplicationUser>.GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetEmailConfirmedAsync(user);
        }

        Task IUserEmailStore<ApplicationUser>.SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SetEmailConfirmedAsync(user, confirmed);
        }

        Task<ApplicationUser> IUserEmailStore<ApplicationUser>.FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return FindByPrimaryKey(normalizedEmail);
        }

        Task<string> IUserEmailStore<ApplicationUser>.GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return GetNormalizedEmailAsync(user);
        }

        Task IUserEmailStore<ApplicationUser>.SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SetNormalizedEmailAsync(user, normalizedEmail);
        }
        #endregion
    }
}
