using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace JamesQMurphy.Auth
{
    public class ApplicationUserStore :
        IDisposable,
        IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>,
        IUserLoginStore<ApplicationUser>,
        IUserRoleStore<ApplicationUser>
    {
        private readonly IApplicationUserStorage _storage;

        private static readonly IList<string> _noRoles = new List<string>();
        private static readonly IList<string> _justRegisteredUserRole = new List<string> { ApplicationRole.RegisteredUser.Name };
        private static readonly IList<string> _adminAndRegisteredUserRoles = new List<string> { ApplicationRole.RegisteredUser.Name, ApplicationRole.Administrator.Name };

        public ApplicationUserStore(IApplicationUserStorage storage)
        {
            _storage = storage;
        }

        #region Helpers
        public Task<string> GetUserIdAsync(ApplicationUser user) => Task.FromResult(user.UserId);
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

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await UpdateAsync(user, cancellationToken);
        }
        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dirtyRecords = user.ApplicationUserRecords.Where(r => r.IsDirty).ToList();
            foreach (var rec in dirtyRecords)
            {
                var updatedRecord = await _storage.SaveAsync(rec, cancellationToken);
                user.AddOrReplaceUserRecord(updatedRecord);
            }
            return IdentityResult.Success;
        }
        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var rec in user.ApplicationUserRecords)
            {
                _ = await _storage.DeleteAsync(rec, cancellationToken);
            }
            return IdentityResult.Success;
        }
        public async Task<ApplicationUser> FindById(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var records = await _storage.FindByIdAsync(userId, cancellationToken);
            return records.Any() ? new ApplicationUser(records) : null;
        }
        public async Task<ApplicationUser> FindByEmailAddress(string normalizedEmailAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var records = await _storage.FindByEmailAddressAsync(normalizedEmailAddress, cancellationToken);
            return records.Any() ? new ApplicationUser(records) : null;
        }
        public async Task<ApplicationUser> FindByUserName(string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var records = await _storage.FindByUserNameAsync(userName, cancellationToken);
            return records.Any() ? new ApplicationUser(records) : null;
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
            return CreateAsync(user, cancellationToken);
        }

        Task<IdentityResult> IUserStore<ApplicationUser>.DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return DeleteAsync(user, cancellationToken);
        }

        Task<ApplicationUser> IUserStore<ApplicationUser>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return FindById(userId, cancellationToken);
        }

        Task<ApplicationUser> IUserStore<ApplicationUser>.FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return FindByUserName(normalizedUserName, cancellationToken);
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
            return UpdateAsync(user, cancellationToken);
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
            return FindByEmailAddress(normalizedEmail, cancellationToken);
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


        #region IUserRoleStore<ApplicationUser> implementation
        public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var result = user.EmailConfirmed ?
                            (user.IsAdministrator ? _adminAndRegisteredUserRoles : _justRegisteredUserRole)
                            : _noRoles;
            return Task.FromResult(result);
        }

        public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            var roles = GetRolesAsync(user, cancellationToken).GetAwaiter().GetResult();
            foreach (var role in roles)
            {
                if (role.ToUpperInvariant() == roleName.ToUpperInvariant())
                    return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IUserLoginStore<ApplicationUser> implementation
        public Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            var newApplicationUserRecord = new ApplicationUserRecord(login.LoginProvider, login.ProviderKey, user.UserId);
            newApplicationUserRecord.SetStringAttribute("providerDisplayName", login.ProviderDisplayName);
            user.AddOrReplaceUserRecord(newApplicationUserRecord);
            return Task.CompletedTask;
        }

        public async Task<ApplicationUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var records = await _storage.FindByProviderAndKeyAsync(loginProvider, providerKey, cancellationToken);
            return records.Any() ? new ApplicationUser(records) : null;
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
