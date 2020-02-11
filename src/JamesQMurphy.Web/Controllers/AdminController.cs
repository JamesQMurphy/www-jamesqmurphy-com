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
    [Authorize(Roles = "Administrator")]
    public class adminController : JqmControllerBase
    {
        private readonly IEmailService _emailService;

        public adminController(IEmailService emailService, WebSiteOptions webSiteOptions) : base(webSiteOptions)
        {
            _emailService = emailService;
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
                var result = await _emailService.SendEmailAsync(
                    model.Email,
                    $"Test message from {WebSiteOptions.WebSiteTitle}",
                    $"This is a test message from {WebSiteOptions.WebSiteTitle}.{Environment.NewLine}{Environment.NewLine}{model.Message}"
                    );

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
    }
}