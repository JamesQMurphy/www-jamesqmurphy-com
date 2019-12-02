using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace JamesQMurphy.Auth
{
    public class InMemoryApplicationUserStorage : IApplicationUserStorage
    {
        private readonly Dictionary<string, ApplicationUser> _dictUsers = new Dictionary<string, ApplicationUser>();

        public InMemoryApplicationUserStorage()
        {
            var user = new ApplicationUser()
            {
                UserId = "+++User+++",
                Email = "user@local",
                EmailConfirmed = true,
                NormalizedEmail = "USER@LOCAL",
                NormalizedUserName = "REGISTEREDUSER",
                UserName = "RegisteredUser"
            };
            user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, "abcde");
            _ = CreateAsync(user, CancellationToken.None).GetAwaiter().GetResult();

            var adminUser = new ApplicationUser()
            {
                UserId = "x+xAdminx+x",
                Email = "admin@local",
                EmailConfirmed = true,
                NormalizedEmail = "ADMIN@LOCAL",
                NormalizedUserName = "ADMINISTRATOR",
                UserName = "Administrator",
                IsAdministrator = true
            };
            adminUser.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, "abcde");
            _ = CreateAsync(adminUser, CancellationToken.None).GetAwaiter().GetResult();

        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_dictUsers.ContainsKey(user.UserId))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError() { Description = "Already present" }));
            }
            _dictUsers.Add(user.UserId, user);
            user.LastUpdated = DateTime.UtcNow;
            return Task.FromResult(IdentityResult.Success);
        }
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            _dictUsers[user.UserId] = user;
            user.LastUpdated = DateTime.UtcNow;
            return Task.FromResult(IdentityResult.Success);
        }
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_dictUsers.Remove(user.UserId))
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError() { Description = "User not found" }));
            }
        }
        public Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_dictUsers.ContainsKey(userId))
            {
                return Task.FromResult(_dictUsers[userId]);
            }
            else
            {
                return Task.FromResult((ApplicationUser)null);
            }
        }

        public Task<ApplicationUser> FindByEmailAddressAsync(string normalizedEmailAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var user = _dictUsers.Values.Where(u => u.NormalizedEmail == normalizedEmailAddress).FirstOrDefault();
            return Task.FromResult(user);
        }

        public Task<ApplicationUser> FindByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var user = _dictUsers.Values.Where(u => u.NormalizedUserName == normalizedUserName).FirstOrDefault();
            return Task.FromResult(user);
        }

        public Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_dictUsers.Values.AsEnumerable());
        }
    }
}
