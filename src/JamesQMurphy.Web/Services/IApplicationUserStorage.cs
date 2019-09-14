using JamesQMurphy.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public interface IApplicationUserStorage
    {
        Task<IdentityResult> CreateAsync(ApplicationUser user);
        Task<IdentityResult> UpdateAsync(ApplicationUser user);
        Task<IdentityResult> DeleteAsync(ApplicationUser user);

        Task<ApplicationUser> FindByEmailAddress(string normalizedEmailAddress);
        Task<ApplicationUser> FindByUserName(string normalizedUserName);
    }
}
