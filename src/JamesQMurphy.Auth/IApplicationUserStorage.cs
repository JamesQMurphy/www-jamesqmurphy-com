using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JamesQMurphy.Auth
{
    public interface IApplicationUserStorage
    {
        Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken));
        Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken));
        Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken));

        Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken));
        Task<ApplicationUser> FindByEmailAddressAsync(string normalizedEmailAddress, CancellationToken cancellationToken = default(CancellationToken));
        Task<ApplicationUser> FindByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}

