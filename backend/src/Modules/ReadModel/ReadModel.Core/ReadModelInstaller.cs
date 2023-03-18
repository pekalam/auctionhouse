using Common.Application.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadModel.Core.Model;
using ReadModel.Core.Services;
using System.Reflection;

namespace ReadModel.Core
{
    public class ReadModelInstaller
    {
        public IServiceCollection Services { get; }

        public ReadModelInstaller(IServiceCollection services, IConfiguration configuration, string sectionName = "MongoDb")
        {
            Services = services;
            AddCore(services, configuration, sectionName);
        }

        private static void AddCore(IServiceCollection services, IConfiguration configuration, string sectionName)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection(sectionName));

            services.AddEventConsumers(typeof(ReadModelInstaller));
            services.AddSingleton<ReadModelDbContext>();
            services.AddAutoMapper(typeof(ReadModelInstaller).Assembly, Assembly.GetEntryAssembly());
        }

        public ReadModelInstaller AddBidRaisedNotifications(Func<IServiceProvider, IBidRaisedNotifications> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public ReadModelInstaller AddAuctionImageReadRepository(Func<IServiceProvider, IAuctionImageReadRepository> factory)
        {
            Services.AddTransient(factory);
            return this;
        }
    }
}
