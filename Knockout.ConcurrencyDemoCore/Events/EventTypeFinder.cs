using SignalR.EventAggregatorProxy.Event;

namespace Knockout.ConcurrencyDemoCore.Events;

public class EventTypeFinder : IEventTypeFinder
{
    private readonly List<Type> _types;

    public EventTypeFinder()
    {
        var type = typeof(IMessage);
        _types = type.Assembly.GetTypes().Where(t => t.IsClass && type.IsAssignableFrom(t)).ToList();
    }


    public IEnumerable<Type> ListEventsTypes()
    {
        return _types;
    }
}