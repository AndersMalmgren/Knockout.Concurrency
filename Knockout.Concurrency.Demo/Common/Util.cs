using System;
using System.ComponentModel;

namespace Knockout.Concurrency.Demo.Common
{
    public static class Util
    {
        public static T ConvertTo<T>(string value)
        {
            if (String.IsNullOrEmpty(value))
                return default(T);

            var converter = TypeDescriptor.GetConverter(typeof(T));

            if (converter == null)
                throw new InvalidOperationException("Could not get converter");

            return (T)converter.ConvertFrom(value);
        }
    }
}
