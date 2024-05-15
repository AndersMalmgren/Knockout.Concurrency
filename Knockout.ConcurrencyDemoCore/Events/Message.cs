namespace Knockout.ConcurrencyDemoCore.Events;

public class Message<T> : IMessage<T>
{
    public required T Data { get; set; }
    public Guid SessionId { get; set; }
}

public interface IMessage<out T> : IMessage
{
    Guid SessionId { get; set; }
}

public interface IMessage
{}