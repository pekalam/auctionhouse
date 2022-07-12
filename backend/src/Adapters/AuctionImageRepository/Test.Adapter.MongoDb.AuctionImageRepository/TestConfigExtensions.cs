using Adapter.MongoDb.AuctionImage;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests
{
    internal static class TestConfigExtensions
    {
        public static ImageDbSettings GetDbSettings(this IConfigurationRoot configuration)
        {
            return configuration.GetSection("AuctionImageRepositoryTests").Get<ImageDbSettings>();
        }
    }
}