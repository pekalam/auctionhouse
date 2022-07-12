using Microsoft.Extensions.Configuration;

namespace Test.Adapter.Hangfire.Auctionhouse
{
    internal static class TestConfigExtensions
    {
        public static string GetHangfireDbConnectionString(this IConfigurationRoot configuration)
        {
            return configuration["AuctionhouseHangfireTests:ConnectionString"];
        }
    }
}
