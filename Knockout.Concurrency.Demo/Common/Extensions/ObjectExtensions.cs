using System.Web.Mvc;

namespace Knockout.Concurrency.Demo.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static JsonResult AsJson(this object obj)
        {
            return new JsonResult
            {
                Data = obj,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}