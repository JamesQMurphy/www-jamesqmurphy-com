using JamesQMurphy.Auth;
using JamesQMurphy.Blog;
using JamesQMurphy.Email;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.AdminViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.Controllers
{
    [Authorize(Roles = JamesQMurphy.Auth.ApplicationRole.ADMINISTRATOR)]
    public class adminController : JqmControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IArticleStore _articleStore;
        private readonly IMarkdownHtmlRenderer _markdownHtmlRenderer;
        private readonly UserManager<ApplicationUser> _userManager;

        public adminController(
            IEmailService emailService,
            IArticleStore articleStore,
            IMarkdownHtmlRenderer markdownHtmlRenderer,
            UserManager<ApplicationUser> userManager,
            WebSiteOptions webSiteOptions) : base(webSiteOptions)
        {
            _emailService = emailService;
            _articleStore = articleStore;
            _markdownHtmlRenderer = markdownHtmlRenderer;
            _userManager = userManager;
        }

        public IActionResult index()
        {
            return View();
        }

        public IActionResult throwerror()
        {
            throw new Exception("This is a test exception");
        }

        [HttpGet]
        public async Task<IActionResult> contact()
        {
            var reactions = await _articleStore.GetArticleReactions(Constants.SLUG_CONTACT_US, latest: true);

            return View(reactions.Select(r=>new ContactUsReaction {
                timestamp = r.TimestampAsString,
                userId = r.AuthorId,
                userName = String.IsNullOrWhiteSpace(r.AuthorName) ? "(anonymous)" : r.AuthorName,
                htmlContent = _markdownHtmlRenderer.RenderHtmlSafe(r.Content, keepLineBreaks: true)
            }));
        }

        [HttpGet]
        public IActionResult testemail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> testemail(TestEmailModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _emailService.SendEmailAsync(new EmailMessage
                {
                    EmailAddress = model.Email,
                    Subject = $"Test message from {WebSiteOptions.WebSiteTitle}",
                    Body = $"This is a test message from {WebSiteOptions.WebSiteTitle}.{Environment.NewLine}{Environment.NewLine}{model.Message}"
                });

                if (result.Success)
                {
                    AlertMessageCollection.AddSuccessAlert($"Email sent to {model.Email}\n{result.Details}", "Email Sent!");
                }
                else
                {
                    AlertMessageCollection.AddDangerAlert(result.Details, "Email Failure");
                }

            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> users()
        {
            var userModels = new List<UserModel>();
            foreach (var au in _userManager.Users)
            {
                userModels.Add(new UserModel
                {
                    userId = au.UserId,
                    userName = au.UserName,
                    email = au.Email ?? "",
                    emailVerified = au.EmailConfirmed,
                    externalLogins = (await _userManager.GetLoginsAsync(au)).Select(login => (login.LoginProvider, login.ProviderDisplayName)),
                    lastUpdatedUtc = au.LastUpdated
                });
            }

            // Sort in decending order
            userModels.Sort((u1, u2) => u2.lastUpdatedUtc.CompareTo(u1.lastUpdatedUtc));

            return View(userModels);
        }
    }
}