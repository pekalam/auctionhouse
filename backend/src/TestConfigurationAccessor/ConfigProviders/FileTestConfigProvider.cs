using Microsoft.Extensions.Configuration;

namespace TestConfigurationAccessor.ConfigProviders
{
    internal class FileTestConfigProvider : ITestConfigProvider
    {
        public bool AddConfiguration(ConfigurationManager builder, string envName, string? prefix, string? suffix)
        {
            if (prefix != "file" || suffix is null)
            {
                return false;
            }

            AddDefaultJsonFile(builder);
            AddJsonFile(builder, suffix);

            return true;
        }

        private static void AddDefaultJsonFile(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("settings.json", optional: true);
        }

        private static void AddJsonFile(IConfigurationBuilder builder, string envName)
        {
            var settingsFileName = $"settings.{envName}.json";
            if (File.Exists(settingsFileName)) // if file does not exists, then default will be used 
            {
                builder.AddJsonFile(settingsFileName);
            }
        }
    }
}