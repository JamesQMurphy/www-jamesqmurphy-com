using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public interface IApplicationUserStorage
    {
        Task<IdentityResult> CreateAsync(ApplicationUser user);
        Task<IdentityResult> UpdateAsync(ApplicationUser user);
        Task<IdentityResult> DeleteAsync(ApplicationUser user);

        Task<ApplicationUser> FindByEmailAddressAsync(string normalizedEmailAddress);
        Task<ApplicationUser> FindByUserNameAsync(string normalizedUserName);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    }
}
