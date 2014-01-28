using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Knockout.Concurrency.Demo.Events
{
    public class Message<T> : MessageBase, IMessage<T>
    {
        public T Data { get; set; }
        public Guid SessionId { get; set; }
    }

    public interface IMessage<out T>
    {
        Guid SessionId { get; set; }
    }

    public abstract class MessageBase 
    {}
}