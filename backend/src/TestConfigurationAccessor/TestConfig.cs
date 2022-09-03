﻿using Microsoft.Extensions.Configuration;

namespace TestConfigurationAccessor
{
    /// <summary>
    /// Provides access to test configuration from provider selected by env name. 
    /// Source of configuration is obtained by checking "ENV" environment variable.
    /// If the environment variable contains prefix (ENV="exampleprefix-suffix") then source of configuration is obtained using provider configured for this prefix.
    /// Default value of "ENV" is "local".
    /// </summary>
    public static class TestConfig
    {
        private const string LocalEnvName = "local";
        private const string AppConfigPrefix = "appcfg";
        private static Lazy<IConfigurationRoot> _instance = new Lazy<IConfigurationRoot>(BuildConfiguration);
        
        /// <summary>
        /// Lazy initialized configuration instance
        /// </summary>
        public static IConfigurationRoot Instance => _instance.Value;


        private static IConfigurationRoot BuildConfiguration()
        {
            var envName = GetEnvironmentName();

            var cfgManager = new ConfigurationManager();

            cfgManager.AddUserSecrets(typeof(TestConfig).Assembly);

            var (prefix, suffix) = (envName.Split('-').Length > 0 ? envName.Split('-')[0] : null, envName.Split('-').Length > 1 ? envName.Split('-')[1] : null);
            if (prefix != null && suffix != null && prefix == (AppConfigPrefix))
            {
                AddAppConfig(cfgManager, "", suffix);
            }
            else
            {
                AddDefaultJsonFile(cfgManager);
                AddJsonFile(cfgManager, envName);
            }

            return cfgManager;
        }

        private static void AddDefaultJsonFile(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("settings.json", optional: true);
        }

        private static void AddJsonFile(IConfigurationBuilder builder, string envName)
        {
            builder.AddJsonFile($"settings.{envName}.json");
        }

        private static void AddAppConfig(IConfigurationBuilder builder, string connectionString, string envName)
        {
            builder.AddAzureAppConfiguration(cfg =>
            {
                cfg.Connect(connectionString);
            });
        }

        private static string GetEnvironmentName()
        {
            return Environment.GetEnvironmentVariable("ENV") ?? LocalEnvName;
        }
    }
}