using Microsoft.Extensions.Configuration;

namespace TestConfigurationAccessor.ConfigProviders
{
    internal class AppConfigurationTestConfigProvider : ITestConfigProvider
    {
        private const string AppConfigPrefix = "appcfg";


        public bool AddConfiguration(ConfigurationManager builder, string envName, string? prefix, string? suffix)
        {
            if (prefix == null || suffix == null || prefix != AppConfigPrefix)
            {
                return false;
            }


            builder.AddAzureAppConfiguration(cfg =>
            {
                cfg.Connect("");
            });

            return true;
        }
    }
}