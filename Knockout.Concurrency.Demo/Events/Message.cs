using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knockout.Concurrency.Demo.Events
{
    public class Message<T>
    {
        public T Data { get; set; }
        public Guid SessionId { get; set; }
    }
}