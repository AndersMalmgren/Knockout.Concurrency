using System;
using Knockout.Concurrency.Demo.Events;
using IEventAggregator = SignalR.EventAggregatorProxy.EventAggregation.IEventAggregator;

namespace Knockout.Concurrency.Demo.Common.SignalR
{
    public class EventAggregatorProxy : IEventAggregator, IHandle<object>
    {
        private Action<object> handler;

        public EventAggregatorProxy(Events.IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        public void Subscribe(Action<object> handler)
        {
            this.handler = handler;
        }

        public void Handle(object message)
        {
            handler(message);
        }
    }
}