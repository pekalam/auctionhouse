using Microsoft.Extensions.Configuration;

namespace TestConfigurationAccessor.ConfigProviders
{
    internal interface ITestConfigProvider
    {
        bool AddConfiguration(ConfigurationManager builder, string envName, string? prefix, string? suffix);
    }
}