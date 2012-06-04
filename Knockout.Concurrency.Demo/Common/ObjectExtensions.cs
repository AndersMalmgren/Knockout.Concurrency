using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Knockout.Concurrency.Demo.Common
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