using Knockout.Concurrency.Demo;
using Knockout.Concurrency.Demo.Events;
using Microsoft.Owin;
using Owin;
using SignalR.EventAggregatorProxy.Owin;


[assembly: OwinStartup(typeof(SignalRConfig))]
namespace Knockout.Concurrency.Demo
{
    public class SignalRConfig
    {
        public static void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            app.MapEventProxy<MessageBase>();
        }
    }
}