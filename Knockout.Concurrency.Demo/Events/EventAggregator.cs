using System.Linq;
using Knockout.Concurrency.Demo.Common.Extensions;

namespace Knockout.Concurrency.Demo.Events
{
    public class EventAggregator : IEventAggregator
    {
        private readonly WeakReferenceList<object> subscribers = new WeakReferenceList<object>(); 
        
        public void Subscribe(object subsriber)
        {
            subscribers.Add(subsriber);
        }

        public void Unsubscribe(object subscriber)
        {
            subscribers.Remove(subscriber);
        }

        public void Publish<T>(T message) where T : class
        {
            var lookupType = typeof (IHandle<T>);

            subscribers.Where(lookupType.IsInstanceOfType)
            .Select(s => s as IHandle<T>)
            .ForEach(s => s.Handle(message));
        }
    }
}
