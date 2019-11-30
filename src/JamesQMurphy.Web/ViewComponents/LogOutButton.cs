using JamesQMurphy.Blog;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JamesQMurphy.Web.ViewComponents
{
    public class LogOutButton : ViewComponent
    {
        public IViewComponentResult Invoke(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}
