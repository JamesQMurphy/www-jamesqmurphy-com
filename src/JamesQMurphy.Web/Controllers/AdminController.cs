using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JamesQMurphy.Email;
using JamesQMurphy.Web.Models;
using JamesQMurphy.Web.Models.AdminViewModels;
using Microsoft.AspNetCore.Authorization;

namespace JamesQMurphy.Web.Controllers
{
    [Authorize]
    public class AdminController : JqmControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly string _siteName;

        public AdminController(IEmailService emailService, WebSiteOptions webSiteOptions)
        {
            _emailService = emailService;
            _siteName = webSiteOptions.WebSiteTitle;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult TestEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestEmail(TestEmailModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _emailService.SendEmailAsync(
                    model.Email,
                    $"Test message from {_siteName}",
                    $"This is a test message from {_siteName}.{Environment.NewLine}{Environment.NewLine}{model.Message}"
                    );

                if (result.Success)
                {
                    ViewData["SuccessMsg"] = $"Email sent to {model.Email}\n{result.Details}";
                }
                else
                {
                    ViewData["FailMsg"] = $"Email failure\n{result.Details}";
                }

            }
            return View();
        }
    }
}