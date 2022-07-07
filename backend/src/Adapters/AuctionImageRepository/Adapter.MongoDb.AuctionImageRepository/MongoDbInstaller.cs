using Adapter.MongoDb.AuctionImage;
using Auctions.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adapter.MongoDb
{
    public static class MongoDbInstaller
    {
        public static void AddMongoDbImageDb(this IServiceCollection services, IConfiguration? configuration = null, ImageDbSettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(ImageDbSettings)).Get<ImageDbSettings>();
            services.AddSingleton(settings);
            services.AddTransient<ImageDbContext>();
            services.AddTransient<IAuctionImageRepository, AuctionImageRepository>();
        }
    }
}