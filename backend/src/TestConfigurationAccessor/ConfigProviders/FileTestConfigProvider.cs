using Microsoft.Extensions.Configuration;

namespace TestConfigurationAccessor.ConfigProviders
{
    internal class FileTestConfigProvider : ITestConfigProvider
    {
        public bool AddConfiguration(ConfigurationManager builder, string envName, string? prefix, string? suffix)
        {
            if (prefix != "local")
            {
                return false;
            }

            AddJsonFile(builder, envName);
            AddDefaultJsonFile(builder);

            return true;
        }

        private static void AddDefaultJsonFile(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("settings.json", optional: true);
        }

        private static void AddJsonFile(IConfigurationBuilder builder, string envName)
        {
            builder.AddJsonFile($"settings.{envName}.json");
        }
    }
}