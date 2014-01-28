using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Ninject;

namespace Knockout.Concurrency.Demo.Common.SignalR
{
    public class SignalRNinjectDependencyResolver : DefaultDependencyResolver
    {
        private readonly IKernel kernel;

        public SignalRNinjectDependencyResolver(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public override object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType) ?? base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType).Concat(base.GetServices(serviceType));
        }
    }
}