using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class InMemoryApplicationUserStorage : IApplicationUserStorage
    {
        private readonly Dictionary<string, ApplicationUser> _dictUsers = new Dictionary<string, ApplicationUser>();

        public InMemoryApplicationUserStorage()
        {
            var user = new ApplicationUser()
            {
                Email = "a@b",
                EmailConfirmed = true,
                NormalizedEmail = "A@B",
                NormalizedUserName = "USERNAME",
                UserName = "Username"
            };

            user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, "abcde");
            _ = CreateAsync(user).GetAwaiter().GetResult();

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
        public Task<ApplicationUser> FindByEmailAddressAsync(string normalizedEmailAddress)
        {
            if (_dictUsers.ContainsKey(normalizedEmailAddress))
            {
                return Task.FromResult(_dictUsers[normalizedEmailAddress]);
            }
            else
            {
                return Task.FromResult((ApplicationUser)null);
            }
        }

        public Task<ApplicationUser> FindByUserNameAsync(string normalizedUserName)
        {
            var user = _dictUsers.Values.Where(u => u.NormalizedUserName == normalizedUserName).FirstOrDefault();
            return Task.FromResult(user);
        }

        public Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return Task.FromResult(_dictUsers.Values.AsEnumerable());
        }
    }
}
