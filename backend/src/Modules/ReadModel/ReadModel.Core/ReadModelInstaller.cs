using Common.Application.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ReadModel.Core.Model;
using ReadModel.Core.Services;
using System.Reflection;

namespace ReadModel.Core
{
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
            services.AddAutoMapper(typeof(ReadModelInstaller).Assembly, Assembly.GetEntryAssembly());
        }

        public ReadModelInstaller AddBidRaisedNotifications(Func<IServiceProvider, IBidRaisedNotifications> factory)
        {
            Services.AddTransient(factory);
            return this;
        }
    }
}
