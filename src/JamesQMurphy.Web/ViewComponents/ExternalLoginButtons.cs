using JamesQMurphy.Web.Models.ViewComponentModels;
using Microsoft.AspNetCore.Mvc;

namespace JamesQMurphy.Web.ViewComponents
{
    public class ExternalLoginButtons : ViewComponent
    {
        public IViewComponentResult Invoke(string caption)
        {
            return View(new ExternalLoginButtonsModel { Caption = caption });
        }
    }
}
