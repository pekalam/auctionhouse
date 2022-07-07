using Auctions.Application.CommandAttributes;
using Auctions.Domain.Services;
using Chronicle;
using Common.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Application
{
    public static class AuctionsInstaller
    {
        public static void AddAuctionsModule(this IServiceCollection services)
        {
            InAuctionCreateSessionAttribute.LoadAuctionCreateSessionCommandMembers(typeof(AuctionsInstaller).Assembly);
            SaveTempAuctionImageAttribute.LoadImagePathCommandMembers(typeof(AuctionsInstaller).Assembly);
            services.AddTransient<AuctionImageService>();
            services.AddTransient<CreateAuctionService>();
            services.AddTransient<AuctionUnlockService>();
            services.AddEventSubscribers(typeof(AuctionsInstaller));
        }
    }
}
