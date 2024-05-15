using SignalR.EventAggregatorProxy.Constraint;

namespace Knockout.ConcurrencyDemoCore.Events
{
    public class MessageConstraintHandler : EventConstraintHandler<IMessage<object>, MessageConstraint>
    {
        public override bool Allow(IMessage<object> message, ConstraintContext context, MessageConstraint constraint) => message.SessionId != constraint.SessionId;
    }

    public class MessageConstraint
    {
        public Guid SessionId { get; set; }
    }
}