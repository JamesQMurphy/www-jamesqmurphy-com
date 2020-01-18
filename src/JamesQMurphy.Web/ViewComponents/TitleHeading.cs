using JamesQMurphy.Web.Models.ViewComponentModels;
using Microsoft.AspNetCore.Mvc;

namespace JamesQMurphy.Web.ViewComponents
{
    public class TitleHeading : ViewComponent
    {
        public IViewComponentResult Invoke(string title)
        {
            return View(new TitleHeadingModel { Title = title });
        }
    }
}
