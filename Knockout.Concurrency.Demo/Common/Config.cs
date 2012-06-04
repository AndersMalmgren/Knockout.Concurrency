using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq.Expressions;
using Knockout.Concurrency.Demo.Common.Extensions;

namespace Knockout.Concurrency.Demo.Common
{
    public class Config : IConfig
    {
        public TimeSpan LongtimePollingTimeout
        {
            get { return GetSetting(() => LongtimePollingTimeout); }
        }


        private TProperty GetSetting<TProperty>(Expression<Func<TProperty>> property)
        {
            var name = property.GetMemberInfo().Name;

            var settings = (NameValueCollection)ConfigurationManager.GetSection("appSettings");
            var value = settings[name];

            if (value == null)
            {
                return default(TProperty);
            }

            var result = Util.ConvertTo<TProperty>(value);

            return result;
        }


    }
}
