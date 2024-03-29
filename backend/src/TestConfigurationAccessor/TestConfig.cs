﻿using Microsoft.Extensions.Configuration;
using TestConfigurationAccessor.ConfigProviders;

namespace TestConfigurationAccessor
{
    /// <summary>
    /// Provides access to test configuration from provider selected by env name. 
    /// Source of configuration is obtained by checking "APP_ENV" environment variable.
    /// If the environment variable contains prefix (ENV="exampleprefix-suffix") then source of configuration is obtained using provider configured for this prefix.
    /// Suffix usage depends on type of provider.
    /// Default value of "APP_ENV" is "file-local" which results in usage of default file-based provider. The file-based provider uses APP_ENV variable to choose source file of settings.
    /// For example: 
    /// - if APP_ENV="file-local", provider loads: settings.json, settings.local.json
    /// - if APP_ENV="file-docker", provider loads: settings.json, settings.docker.json
    /// </summary>
    public static class TestConfig
    {
        private const string LocalEnvName = "file-local";
        private static Lazy<IConfigurationRoot> _instance = new Lazy<IConfigurationRoot>(BuildConfiguration);

        private static readonly IEnumerable<ITestConfigProvider> TestConfigProviders = new ITestConfigProvider[]
        {
            new FileTestConfigProvider(),
            new AppConfigurationTestConfigProvider()
        };

        /// <summary>
        /// Lazy initialized configuration instance
        /// </summary>
        public static IConfigurationRoot Instance => _instance.Value;


        private static IConfigurationRoot BuildConfiguration()
        {
            var envName = GetEnvironmentName();
            var cfgManager = new ConfigurationManager();
            try
            {
                cfgManager.AddUserSecrets(typeof(TestConfig).Assembly);
            }catch(Exception) { }

            var (prefix, suffix) = (envName.Split('-').Length > 0 ? envName.Split('-')[0] : null, envName.Split('-').Length > 1 ? envName.Split('-')[1] : null);
            foreach (var testConfigProvider in TestConfigProviders)
            {
                if (testConfigProvider.AddConfiguration(cfgManager, envName, prefix, suffix))
                {
                    break;
                }
            }

            return cfgManager;
        }

        private static string GetEnvironmentName()
        {
            return Environment.GetEnvironmentVariable("APP_ENV") ?? LocalEnvName;
        }
    }
}