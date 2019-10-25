﻿using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.AccountViewModels;
using JamesQMurphy.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace JamesQMurphy.Web.Controllers
{
    public class AccountController : JqmControllerBase
    {
        private readonly ApplicationSignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailGenerator _emailGenerator;

        public AccountController
        (
            ApplicationSignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IEmailGenerator emailGenerator
        )
        {
            _signInManager = signInManager;
            _logger = logger;
            _emailGenerator = emailGenerator;
            _userManager = _signInManager.UserManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // If email is not verified, fail the login
                var user = await _userManager.FindByEmailAsync(_userManager.NormalizeKey(model.Email));
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

        [HttpPost]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewData["IsLoggedIn"] = IsLoggedIn;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewData["IsLoggedIn"] = IsLoggedIn;
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
                    await _emailGenerator.GenerateEmailAsync(userFromEmail, EmailType.EmailAlreadyRegistered);
                }

                async Task<IdentityResult> resetPasswordAsync(ApplicationUser user)
                {
                    return await _userManager.ResetPasswordAsync(
                        user,
                        await _userManager.GeneratePasswordResetTokenAsync(user),
                        pwForm);
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
                    result = await resetPasswordAsync(userFromEmail);
                }
                else
                {
                    if (userFromUserName != null)
                    {
                        result = IdentityResult.Failed(new IdentityError { Description = "That UserName is already taken.  Please choose another." });
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
                                result = await resetPasswordAsync(userFromEmail);
                            }
                        }
                    }
                }

                if (result.Succeeded)
                {

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(userFromEmail);

                    // Note that Url is null when we create the controller as part of a unit test
                    var link = Url?.Action(nameof(AccountController.ConfirmEmail), "account", new { userFromEmail.UserName, code }, Request.Scheme);
                    await _emailGenerator.GenerateEmailAsync(userFromEmail, EmailType.EmailVerification, link);

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


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userName, string code)
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
        public IActionResult ForgotPassword()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Reset Your Password";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user?.EmailConfirmed == true)
                {
                    // User exists and email is confirmed; generate a password reset link
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    // Note that Url is null when we create the controller as part of a unit test
                    var link = Url?.Action(nameof(AccountController.ResetPassword), "account", new { user.UserName, code }, Request.Scheme);
                    await _emailGenerator.GenerateEmailAsync(user, EmailType.PasswordReset, link);
                }

                // Even if user doesn't exist, show the confirmation page
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Check Your Email";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null, string username = null)
        {
            if (code == null || username == null)
            {
                return RedirectToAction(nameof(ForgotPassword));
            }
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Enter New Password";
            var model = new ResetPasswordViewModel { Code = code, Username = username };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
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
                        await _emailGenerator.GenerateEmailAsync(user, EmailType.PasswordChanged);
                    }
                }
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
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
        public IActionResult ResetPasswordConfirmation()
        {
            ViewData[Constants.VIEWDATA_PAGETITLE] = "Password Changed";
            return View();
        }
    }
}