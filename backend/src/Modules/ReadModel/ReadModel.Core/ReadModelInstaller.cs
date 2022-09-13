using Common.Application;
using Microsoft.Extensions.DependencyInjection;
using ReadModel.Core.Model;
using ReadModel.Core.Services;

namespace ReadModel.Core
{
    public static class ReadModelInstallerX
    {
        public static void AddReadModel(this IServiceCollection services, MongoDbSettings mongoDbSettings)
        {
            services.AddEventConsumers(typeof(ReadModelInstaller));
            services.AddSingleton(mongoDbSettings);
            services.AddSingleton<ReadModelDbContext>();
        }
    }

    public class ReadModelInstaller
    {
        public IServiceCollection Services { get; }

        public ReadModelInstaller(IServiceCollection services, MongoDbSettings mongoDbSettings)
        {
            Services = services;
            AddCore(services, mongoDbSettings);
        }

        private static void AddCore(IServiceCollection services, MongoDbSettings mongoDbSettings)
        {
            services.AddEventConsumers(typeof(ReadModelInstaller));
            services.AddSingleton(mongoDbSettings);
            services.AddSingleton<ReadModelDbContext>();
        }

        public ReadModelInstaller AddBidRaisedNotifications(Func<IServiceProvider, IBidRaisedNotifications> factory)
        {
            Services.AddTransient(factory);
            return this;
        }
    }
}
