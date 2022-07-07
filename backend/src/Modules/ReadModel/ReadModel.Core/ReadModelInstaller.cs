using Common.Application;
using Microsoft.Extensions.DependencyInjection;
using ReadModel.Core.Model;

namespace ReadModel.Core
{
    public static class ReadModelInstaller
    {
        public static void AddReadModel(this IServiceCollection services, MongoDbSettings mongoDbSettings)
        {
            services.AddEventConsumers(typeof(ReadModelInstaller));
            services.AddSingleton(mongoDbSettings);
            services.AddSingleton<ReadModelDbContext>();
        }
    }
}
