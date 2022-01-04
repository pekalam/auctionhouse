using Auctions.Domain.Services;
using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Auctions.Application
{
    public static class AuctionsInstaller
    {
        public static void AddAuctionsModule(this IServiceCollection services)
        {
            services.AddTransient<AuctionImageService>();
            services.AddTransient<CreateAuctionService>();
            services.AddTransient<AuctionUnlockService>();
            services.AddEventSubscribers(typeof(AuctionsInstaller));
            services.AddChronicle(build =>
            {
                build.UseInMemoryPersistence();
            });
        }
    }
}
