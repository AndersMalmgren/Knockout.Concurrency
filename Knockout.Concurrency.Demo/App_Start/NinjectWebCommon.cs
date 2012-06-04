using System.Web.Mvc;
using Knockout.Concurrency.Demo.Common;
using Knockout.Concurrency.Demo.Common.Filters;
using Knockout.Concurrency.Demo.Events;
using Knockout.Concurrency.Demo.Models;
using Ninject.Web.Mvc.FilterBindingSyntax;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Knockout.Concurrency.Demo.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(Knockout.Concurrency.Demo.App_Start.NinjectWebCommon), "Stop")]

namespace Knockout.Concurrency.Demo.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.BindFilter<HandleTimeout>(FilterScope.Global, 0);
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            kernel.Bind<IEventQueue<Dog>>().To<EventQueue<Dog>>().InSingletonScope();
            kernel.Bind<IConfig>().To<Config>();
        }        
    }
}
