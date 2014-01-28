using System;
using Knockout.Concurrency.Demo.Events;
using SignalR.EventAggregatorProxy.Constraint;

namespace Knockout.Concurrency.Demo.Common.SignalR.ConstraintHandlers
{
    public class MessageConstraintHandler : EventConstraintHandler<IMessage<object>>
    {
        public override bool Allow(IMessage<object> message, string username, dynamic constraint)
        {
            return message.SessionId.ToString() != constraint.sessionId.Value;
        }
    }
}