using Microsoft.Extensions.Configuration;

namespace Common.WebAPI.Configuration
{
    public static class ConfigurationUtils
    {
        private const string AzureClientSecretKey = "AZURE_CLIENT_SECRET";
        private const string AzureClientIdKey = "AZURE_CLIENT_ID";
        private const string AzureTenantKey = "AZURE_TENANT_ID";
        private const string EnvironmentKey = "ENV";
        private const string AppConfigurationConnectionStringKey = "AppConfigurationProd";

        public const string LocalEnvName = "local";

        public static void InitializeAzureCredentials(IConfiguration configuration)
        {
            var azureCredentials = configuration.GetSection("AzureCredentials");
            if (!azureCredentials.AsEnumerable().Any())
            {
                return;
            }

            Environment.SetEnvironmentVariable(AzureClientSecretKey, azureCredentials[AzureClientSecretKey]);
            Environment.SetEnvironmentVariable(AzureClientIdKey, azureCredentials[AzureClientIdKey]);
            Environment.SetEnvironmentVariable(AzureTenantKey, azureCredentials[AzureTenantKey]);
        }

        public static string? GetEnvironmentName()
        {
            return Environment.GetEnvironmentVariable(EnvironmentKey);
        }

        public static string GetAppConfigurationConnectionString(IConfiguration configuration)
        {
            return configuration.GetConnectionString(AppConfigurationConnectionStringKey);
        }
    }
}
