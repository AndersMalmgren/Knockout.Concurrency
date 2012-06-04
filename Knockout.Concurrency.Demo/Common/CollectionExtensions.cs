using System;
using System.Collections.Generic;

namespace Knockout.Concurrency.Demo.Common
{
    public static class CollectionExtensions 
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach(var item in collection)
            {
                action(item);
            }

            return collection;
        }
    }
}
