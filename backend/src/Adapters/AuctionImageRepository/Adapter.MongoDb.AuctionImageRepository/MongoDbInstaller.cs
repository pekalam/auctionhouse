using Adapter.MongoDb.AuctionImage;
using Auctions.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadModel.Core;

namespace Adapter.MongoDb
{
    public static class MongoDbInstaller
    {
        public static void AddMongoDbImageReadRepositoryAdapter(this ReadModelInstaller installer, IConfiguration? configuration = null, ImageDbSettings? settings = null)
        {
            installer.Services.AddMongoDbImageDb(configuration, settings);
            installer.Services.AddTransient<AuctionImageRepository>();

            installer.AddAuctionImageReadRepository(prov => prov.GetRequiredService<AuctionImageRepository>());
        }

        public static AuctionsDomainInstaller AddMongoDbImageRepositoryAdapter(this AuctionsDomainInstaller installer, IConfiguration? configuration = null, ImageDbSettings? settings = null)
        {
            installer.Services.AddMongoDbImageDb(configuration, settings);
            installer.Services.AddTransient<AuctionImageRepository>();

            installer.AddAuctionImageRepository((prov) => prov.GetRequiredService<AuctionImageRepository>());
            return installer;
        }

        private static void AddMongoDbImageDb(this IServiceCollection services, IConfiguration? configuration = null, ImageDbSettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(ImageDbSettings)).Get<ImageDbSettings>();
            services.AddSingleton(settings);
            services.AddTransient<ImageDbContext>();
        }
    }
}