using Microsoft.Extensions.Configuration;
using RabbitMq.EventBus;

namespace Test.Adapter.RabbitMq.EventBus
{
    internal static class TestConfigExtensions
    {
        public static RabbitMqSettings GetRabbitMqSettings(this IConfigurationRoot configuration)
        {
            return new RabbitMqSettings
            {
                ConnectionString = configuration["EventBusTests:ConnectionString"]
            };
        }
    }
}
