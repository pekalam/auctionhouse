using Adapter.MongoDb.AuctionImage;
using Auctions.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Adapter.MongoDb
{
    public static class MongoDbInstaller
    {
        public static void AddMongoDb(this IServiceCollection services, ImageDbSettings settings)
        {
            services.AddSingleton(settings);
            services.AddTransient<ImageDbContext>();
            services.AddTransient<IAuctionImageRepository, AuctionImageRepository>();
        }
    }
}