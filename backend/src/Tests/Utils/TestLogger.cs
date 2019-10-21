using System;
using System.Text.RegularExpressions;
using EasyNetQ.Logging;
using NUnit.Framework;

namespace Infrastructure.Tests.Utils
{
    public class TestLogger : ILogProvider
    {
        private Regex reg = new Regex(@"{\w+}", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public Logger GetLogger(string name)
        {
            return (LogLevel level, Func<string> func, Exception exception, object[] parameters) =>
            {
                string s;
                if (func != null)
                {
                    s = func();
                    s = reg.Replace(s, "{w}", 1000);
                    int ind = s.IndexOf("{w}");
                    int i = 0;
                    while (ind != -1)
                    {
                        s = s.Remove(ind, 3);
                        s = s.Insert(ind, $"{{{i++}}}");
                        ind = s.IndexOf("{w}");
                    }

                    TestContext.Out.WriteLine(String.Format(s, parameters));
                }

                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            return null;
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            return null;
        }
    }
}