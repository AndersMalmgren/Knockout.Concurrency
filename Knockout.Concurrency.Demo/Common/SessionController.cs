using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Knockout.Concurrency.Demo.Common
{
    public class SessionController : AsyncController
    {
        public Guid SessionId { get; set; }
    }
}