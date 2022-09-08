﻿using Adapter.MongoDb.AuctionImage;
using Auctions.Domain;
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
        }

        public static AuctionsDomainInstaller AddMongoDbImageRepositoryAdapter(this AuctionsDomainInstaller installer, IConfiguration? configuration = null, ImageDbSettings? settings = null)
        {
            installer.Services.AddMongoDbImageDb(configuration, settings);
            installer.Services.AddTransient<AuctionImageRepository>();

            installer.AddAuctionImageRepository((prov) => prov.GetRequiredService<AuctionImageRepository>());
            return installer;
        }
    }
}