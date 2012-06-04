namespace Knockout.Concurrency.Demo.Events
{
    public interface IHandle<T> where T : class
    {
        void Handle(T message);
    }
}
