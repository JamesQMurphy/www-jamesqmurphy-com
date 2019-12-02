using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Services
{
    public class ApplicationSignInManager<TUser> : SignInManager<TUser> where TUser : class
    {
        public ApplicationSignInManager(
            UserManager<TUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<TUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<TUser>> logger,
            IAuthenticationSchemeProvider schemes)
                : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes)
        {
        }

        // overridden to use email address instead of username
        public override async Task<SignInResult> PasswordSignInAsync(string emailAddress, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var user = await UserManager.FindByEmailAsync(emailAddress);
            if (user == null)
            {
                return SignInResult.Failed;
            }

            return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }
    }
}
