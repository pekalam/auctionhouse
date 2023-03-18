using Microsoft.Extensions.Configuration;
using ReadModel.Core.Model;

namespace ReadModel.Tests.Integration
{
    internal static class TestConfigExtensions
    {
        public static MongoDbSettings GetMongoDbSettings(this IConfigurationRoot config)
        {
            return config.GetSection("ReadModelTests:ReadModelSettings").Get<MongoDbSettings>();
        }
    }
}
