using NUnit.Framework;

namespace IntegrationTests
{
    public class TestContextUtils
    {
        public static string GetParameterOrDefault(string key, string defaultValue)
        {
            return TestContext.Parameters.Exists(key) ? TestContext.Parameters[key] : defaultValue;
        }
    }
}