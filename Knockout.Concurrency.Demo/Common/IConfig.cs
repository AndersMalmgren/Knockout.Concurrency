using System;

namespace Knockout.Concurrency.Demo.Common
{
    public interface IConfig
    {
        TimeSpan LongtimePollingTimeout { get; }
    }
}