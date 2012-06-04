using System;
using System.Web.Mvc;
using Knockout.Concurrency.Demo.Common.Extensions;

namespace Knockout.Concurrency.Demo.Common.Filters
{
    public class HandleTimeout : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if(filterContext.Exception is TimeoutException && filterContext.Controller is SessionController)
            {
                filterContext.HttpContext.Response.StatusCode = 200;
                filterContext.Result = new { Timeout = true }.AsJson();
                filterContext.ExceptionHandled = true;
            }

            base.OnException(filterContext);
        }
    }
}
