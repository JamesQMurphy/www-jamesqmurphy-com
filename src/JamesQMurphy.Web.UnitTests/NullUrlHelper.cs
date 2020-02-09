using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace JamesQMurphy.Web.UnitTests
{
    public class NullUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext => throw new NotImplementedException();

        public string Action(UrlActionContext actionContext) => "";

        public string Content(string contentPath) => "";

        public bool IsLocalUrl(string url) => true;

        public string Link(string routeName, object values) => "";

        public string RouteUrl(UrlRouteContext routeContext) => "";
    }
}
