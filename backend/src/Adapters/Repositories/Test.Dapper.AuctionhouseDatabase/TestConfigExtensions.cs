using Adapter.Dapper.AuctionhouseDatabase;
using Microsoft.Extensions.Configuration;

namespace Test.Dapper.AuctionhouseDatabase
{
    internal static class TestConfigExtensions
    {
        public static AuctionhouseRepositorySettings GetRepositorySettings(this IConfigurationRoot configuration)
        {
            return configuration.GetSection("AuctionBidsRepositoryTests").Get<AuctionhouseRepositorySettings>();
        }
    }
}
