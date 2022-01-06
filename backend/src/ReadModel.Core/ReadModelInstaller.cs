using Common.Application;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.EventBus;
using ReadModel.Core.Model;

namespace ReadModel.Core
{
    public static class ReadModelInstaller
    {
        public static void AddReadModel(this IServiceCollection services, MongoDbSettings mongoDbSettings, RabbitMqSettings rabbitMqSettings)
        {
            services.AddEventConsumers(typeof(ReadModelInstaller));
            services.AddRabbitMq(rabbitMqSettings);
            services.AddSingleton(mongoDbSettings);
            services.AddSingleton<ReadModelDbContext>();
        }

        public static void InitSubscribers(IServiceProvider serviceProvider)
        {
            RabbitMqInstaller.InitializeEventConsumers(serviceProvider, typeof(ReadModelInstaller).Assembly);
        }
    }
}
