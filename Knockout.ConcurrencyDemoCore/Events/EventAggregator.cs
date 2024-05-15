namespace Knockout.ConcurrencyDemoCore.Events;

public interface IEventAggregator : SignalR.EventAggregatorProxy.EventAggregation.IEventAggregator
{
    Task Publish<T>(T message) where T : class;
}

public class EventAggregator : IEventAggregator
{
    private Func<object, Task>? _handler;

    public void Subscribe(Func<object, Task>? handler)
    {
        _handler = handler;
    }

    public async Task Publish<T>(T message) where T : class
    {
        if (_handler != null)
            await _handler(message);
    }
}