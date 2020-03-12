using JamesQMurphy.Auth;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.AccountViewModels;
using JamesQMurphy.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace JamesQMurphy.Web.Controllers
{
    [Authorize]
    public class accountController : JqmControllerBase
    {
        private readonly ApplicationSignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailGenerator _emailGenerator;

        private readonly string USER_ALREADY_TAKEN_MESSAGE = "That UserName is already taken.  Please choose another.";

        public accountController
        (
            ApplicationSignInManager<ApplicationUser> signInManager,
            ILogger<accountController> logger,
            IEmailGenerator emailGenerator,
            WebSiteOptions webSiteOptions
        ) : base(webSiteOptions)
        {
            _signInManager = signInManager;
            _logger = logger;
            _emailGenerator = emailGenerator;
            _userManager = _signInManager.UserManager;
        }

        [HttpGet]
        public IActionResult accessdenied()
        {
            return Unauthorized();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult myclaims()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            ViewData[Constants.VIEWDATA_NOPRIVACYCONSENT] = true;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData[Constants.VIEWDATA_NOPRIVACYCONSENT] = true;
            if (ModelState.IsValid)
            {
                // If email is not verified, fail the login
                var user = await _userManager.FindByEmailAsync(model.Email);
                if ((user != null) && user.EmailConfirmed)
                {
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return RedirectToLocal(returnUrl);
                    }
                    // TODO: cleanup
                    //if (result.RequiresTwoFactor)
                    //{
                    //    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                    //}
                    //if (result.IsLockedOut)
                    //{
                    //    _logger.LogWarning("User account locked out.");
                    //    return RedirectToAction(nameof(Lockout));
                    //}
                }
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            return View(model);
        }

        /// <summary>
        /// This is where the "Sign in with Twitter/GitHub" buttons post.  The method redirects the
        /// browser to the appropriate provider's OAuth page, passing along the callback URL.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult loginexternal(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(loginexternalcallback), "account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// This is where the external providers send the browser after they've authenticated (or *not*
        /// authenticated) the user.  If they've been authenticated, we sign them in if they've already
        /// registered, or redirect them to the registration page if they're new.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <param name="remoteError"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> loginexternalcallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogError($"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogDebug($"GetExternalLoginInfoAsync() returned null");
                return RedirectToAction(nameof(login));
            }

            _logger.LogInformation($"Callback from LoginProvider={info.LoginProvider} ProviderKey={info.ProviderKey} ProviderDisplayName={info.ProviderDisplayName}");
            foreach (var claim in info.Principal.Claims)
            {
                _logger.LogDebug($"Claim: Type={claim.Type} Value={claim.Value} Issuer={claim.Issuer}");
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("Login succeeded; user already has login");
                // TODO: fetch image

                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                // TODO: handle locked out case
                _logger.LogInformation("User is locked out");
                return RedirectToAction(nameof(login));
            }
            else
            {
                _logger.LogInformation("Need to create login for user; redirecting");
                return RedirectToAction(nameof(registerexternal));
            }
        }


        [HttpPost]
        public async Task<IActionResult> logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            AlertMessageCollection.AddSuccessAlert($"You have successfully signed out of {WebSiteOptions.WebSiteTitle}.");
            return RedirectToLocal(returnUrl);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult register()
        {
            ViewData["IsLoggedIn"] = IsLoggedIn;
            ViewData[Constants.VIEWDATA_NOPRIVACYCONSENT] = true;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> register(RegisterWithEmailViewModel model)
        {
            ViewData["IsLoggedIn"] = IsLoggedIn;
            ViewData[Constants.VIEWDATA_NOPRIVACYCONSENT] = true;
            var pwForm = model.Password;
            model.Password = String.Empty;
            model.ConfirmPassword = String.Empty;

            if (ModelState.IsValid)
            {
                IdentityResult result = null;
                var userFromEmail = await _userManager.FindByEmailAsync(model.Email);
                var userFromUserName = await _userManager.FindByNameAsync(model.UserName);

                // If using somebody else's confirmed e-mail address, send a warning to that e-mail address
                // TODO: log this
                if (userFromEmail?.EmailConfirmed == true)
                {
                    await _emailGenerator.GenerateEmailAsync(userFromEmail.Email, EmailType.EmailAlreadyRegistered);
                }

                // Very specific condition of a user trying to re-register with exactly
                // the same information.  Reset the password.
                if (userFromUserName != null &&  userFromEmail != null &&
                    userFromUserName.EmailConfirmed == false &&
                    userFromEmail.EmailConfirmed == false &&
                    userFromUserName.NormalizedEmail == userFromEmail.NormalizedEmail &&
                    userFromUserName.NormalizedUserName == userFromEmail.NormalizedUserName
                )
                {
                    result = await _signInManager.ChangeUserPasswordAsync(userFromEmail, pwForm);
                }
                else
                {
                    if (userFromUserName != null)
                    {
                        result = IdentityResult.Failed(new IdentityError { Description = USER_ALREADY_TAKEN_MESSAGE });
                    }
                    else
                    {
                        if (userFromEmail == null)
                        {
                            userFromEmail = new ApplicationUser
                            {
                                UserName = model.UserName,
                                Email = model.Email
                            };
                            result = await _userManager.CreateAsync(userFromEmail, pwForm);
                        }
                        else
                        {
                            if (userFromEmail.EmailConfirmed)
                            {
                                // We've warned the real user; pretend like nothing happened
                                // but we need to short-circuit the success
                                return View("RegisterConfirmation", model);
                            }
                            else
                            {
                                result = await _signInManager.ChangeUserPasswordAsync(userFromEmail, pwForm);
                            }
                        }
                    }
                }

                if (result.Succeeded)
                {

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(userFromEmail);

                    // Note that Url is null when we create the controller as part of a unit test
                    var link = Url?.Action(nameof(accountController.confirmemail), "account", new { userFromEmail.UserName, code }, Request.Scheme);
                    await _emailGenerator.GenerateEmailAsync(userFromEmail.Email, EmailType.EmailVerification, link);

                    // Note that we do *not* sign in the user

                    return View("RegisterConfirmation", model);
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError(String.Empty, error.Description);
                    }
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }

        /// <summary>
        /// The browser gets redirected here from <ref>loginexternalcallback</ref>, if they've
        /// been authenticated but they're not registered
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> registerexternal(string returnUrl = null)
        {
            if (IsLoggedIn)
            {
                return RedirectToAction(nameof(register));
            }
            var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                _logger.LogDebug($"GetExternalLoginInfoAsync() returned null");
                return RedirectToAction(nameof(register));
            }

            var usernameOnExternalSystem = externalLoginInfo.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";

            var userFromProposedUsername = string.IsNullOrWhiteSpace(usernameOnExternalSystem) ? null : await _userManager.FindByNameAsync(usernameOnExternalSystem);
            var model = new RegisterUsernameViewModel
            {
                UserName = usernameOnExternalSystem
            };

            if (userFromProposedUsername != null)
            {
                ModelState.AddModelError(String.Empty, USER_ALREADY_TAKEN_MESSAGE);
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData[Constants.VIEWDATA_NOPRIVACYCONSENT] = true;
            ViewData[Constants.VIEWDATA_EXTERNALPROVIDERNAME] = externalLoginInfo.ProviderDisplayName;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> registerexternal(RegisterUsernameViewModel model, string returnUrl = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (IsLoggedIn)
            {
                return RedirectToAction(nameof(register));
            }
            var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                _logger.LogDebug($"GetExternalLoginInfoAsync() returned null");
                return RedirectToAction(nameof(register));
            }

            IdentityResult result = IdentityResult.Failed();
            if (ModelState.IsValid)
            {
                // Check if username has been taken
                // TODO: Should just let CreateAsync return an error, not this if/then stuff
                _logger.LogDebug($"Model valid; calling FindByNameAsyc({model.UserName})");
                var userFromProposedUsername = await _userManager.FindByNameAsync(model.UserName);
                if (userFromProposedUsername != null)
                {
                    _logger.LogDebug($"Username {model.UserName} already exists; redisplay form");
                    result = IdentityResult.Failed(new IdentityError { Description = USER_ALREADY_TAKEN_MESSAGE });
                }
                else
                {
                    // Create the new user (without a password) and add the external login
                    _logger.LogDebug($"Creating username {model.UserName}");
                    var newUser = new ApplicationUser { UserName = model.UserName };
                    result = await _userManager.CreateAsync(newUser);
                    if (result.Succeeded)
                    {
                        _logger.LogDebug($"User created; adding login for external provider {externalLoginInfo.LoginProvider}({externalLoginInfo.ProviderDisplayName})");
                        result = await _userManager.AddLoginAsync(newUser, externalLoginInfo);
                    }
                }
            }

            if (result.Succeeded)
            {
                // Actually log in as the user
                var signInresult = await _signInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                if (signInresult.Succeeded)
                {
                    _logger.LogDebug("Login succeeded; newly created login");
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    _logger.LogWarning($"Login failed for new user {externalLoginInfo.LoginProvider} {externalLoginInfo.ProviderKey}");
                    return RedirectToAction(nameof(login));
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(String.Empty, error.Description);
                }
            }

            // If we got this far, something failed; redisplay form
            ViewData["ReturnUrl"] = returnUrl;
            ViewData[Constants.VIEWDATA_NOPRIVACYCONSENT] = true;
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> confirmemail(string userName, string code)
        {
            if (userName == null || code == null)
            {
                return RedirectToLocal();
            }

            var user = await _userManager.FindByNameAsync(userName);

            IdentityResult result;
            if (user == null)
            {
                result = IdentityResult.Failed(new IdentityError { Description = "Could not confirm" });
            }
            else
            {
                result = await _userManager.ConfirmEmailAsync(user, code);
            }
            //TODO: Log the result

            if (result.Succeeded)
            {
                ViewData[Constants.VIEWDATA_PAGETITLE] = "Email Confirmed";
                ViewData["Success"] = true;
            }
            else
            {
                ViewData[Constants.VIEWDATA_PAGETITLE] = "Could Not Confirm Your Email";
                ViewData["Success"] = false;
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult forgotpassword()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Reset Your Password";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> forgotpassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user?.EmailConfirmed == true)
                {
                    // User exists and email is confirmed; generate a password reset link
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    // Note that Url is null when we create the controller as part of a unit test
                    var link = Url?.Action(nameof(accountController.resetpassword), "account", new { user.UserName, code }, Request.Scheme);
                    await _emailGenerator.GenerateEmailAsync(user.Email, EmailType.PasswordReset, link);
                }

                // Even if user doesn't exist, show the confirmation page
                return RedirectToAction(nameof(forgotpasswordconfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult forgotpasswordconfirmation()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Check Your Email";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult resetpassword(string code = null, string username = null)
        {
            if (code == null || username == null)
            {
                return RedirectToAction(nameof(forgotpassword));
            }
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Enter New Password";
            var model = new ResetPasswordViewModel { Code = code, Username = username };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> resetpassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = IdentityResult.Failed(new IdentityError() { Code = "InvalidToken" });
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                    if (result.Succeeded)
                    {
                        await _emailGenerator.GenerateEmailAsync(user.Email, EmailType.PasswordChanged);
                    }
                }
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(resetpasswordconfirmation));
                }
                foreach (var error in result.Errors)
                {
                    if (error.Code == "InvalidToken")
                    {
                        error.Description = "There was a problem updating your password.  Either the link we emailed you has expired, or it was not pasted properly into the browser.";
                    }
                    ModelState.AddModelError(String.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult resetpasswordconfirmation()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Password Changed";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> changepassword()
        {
            var hasPassword = await _userManager.HasPasswordAsync(await GetApplicationUserAsync(_userManager));
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Change Your Password";
            return hasPassword ? View() : View("changePassword_externalLogin");
        }

        [HttpPost]
        public async Task<IActionResult> changepassword(ChangePasswordViewModel model)
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Change Your Password";
            if (ModelState.IsValid)
            {
                var user = await GetApplicationUserAsync(_userManager);
                if (await _userManager.HasPasswordAsync(user))
                {
                    var pwVerificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);
                    if (pwVerificationResult == PasswordVerificationResult.Failed)
                    {
                        ModelState.AddModelError(String.Empty, "The password you entered did not match the password we currently have");
                    }
                    else
                    {
                        var result = await _signInManager.ChangeUserPasswordAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            await _emailGenerator.GenerateEmailAsync(user.Email, EmailType.PasswordChanged);
                            return RedirectToAction(nameof(changepasswordconfirmation));
                        }
                        else
                        {
                            _logger.LogError($"Error changing password: {result.ToString()}");
                            ModelState.AddModelError(String.Empty, "There was a problem changing your password.  Please try again later.");
                        }
                    }
                }
                else
                {
                    return View("changePassword_externalLogin");
                }
            }

            // If we got this far, something failed, redisplay (blank) form
            return View(new ChangePasswordViewModel());
        }

        [HttpGet]
        public IActionResult changepasswordconfirmation()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Password Changed";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> mypersonaldata()
        {
            var userRecordsUnsorted = (await GetApplicationUserAsync(_userManager)).ApplicationUserRecords;
            var userRecords = new List<ApplicationUserRecord>();
            userRecords.Add(userRecordsUnsorted.Where(r => r.Provider == ApplicationUserRecord.RECORD_TYPE_ID).FirstOrDefault());
            userRecords.AddRange(userRecordsUnsorted.Where(r => r.Provider != ApplicationUserRecord.RECORD_TYPE_ID));
            return View(userRecords);
        }
    }
}
