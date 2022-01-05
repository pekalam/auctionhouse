using Auctions.Application.CommandAttributes;
using Auctions.Domain.Services;
using Chronicle;
using Common.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Application
{
    public static class AuctionsInstaller
    {
        public static void AddAuctionsModule(this IServiceCollection services, params string[] commandAssemblyNames)
        {
            if (commandAssemblyNames.Length == 0)
            {
                throw new ArgumentException(nameof(commandAssemblyNames));
            }
            InAuctionCreateSessionAttribute.LoadAuctionCreateSessionCommandMembers(commandAssemblyNames);
            SaveTempAuctionImageAttribute.LoadImagePathCommandMembers(commandAssemblyNames);
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
