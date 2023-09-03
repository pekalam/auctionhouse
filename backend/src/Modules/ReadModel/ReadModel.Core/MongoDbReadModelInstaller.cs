using Common.Application.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadModel.Contracts;
using ReadModel.Core.Model;
using System.Reflection;

namespace ReadModel.Core
{
    public static class MongoDbReadModelInstaller
    {
        public static ReadModelInstaller AddMongoDbReadModel(this ReadModelInstaller installer, IConfiguration configuration, string sectionName = "MongoDb")
        {
            installer.Services.Configure<MongoDbSettings>(configuration.GetSection(sectionName));

            installer.Services.AddEventConsumers(typeof(MongoDbReadModelInstaller));
            installer.Services.AddSingleton<ReadModelDbContext>();
            installer.Services.AddAutoMapper(typeof(MongoDbReadModelInstaller).Assembly, typeof(ReadModelInstaller).Assembly, Assembly.GetEntryAssembly());

            return installer;
        }
    }
}
