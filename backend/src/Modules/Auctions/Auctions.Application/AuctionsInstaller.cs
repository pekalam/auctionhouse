using Auctions.Application.CommandAttributes;
using Common.Application;
using Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Application
{
    public class AuctionsApplicationInstaller
    {
        public AuctionsApplicationInstaller(IServiceCollection services)
        {
            Services = services;
            AddCoreServices(services);
        }

        public IServiceCollection Services { get; }

        private static void AddCoreServices(IServiceCollection services)
        {
            InAuctionCreateSessionAttribute.LoadAuctionCreateSessionCommandMembers(typeof(AuctionsApplicationInstaller).Assembly);
            SaveTempAuctionImageAttribute.LoadImagePathCommandMembers(typeof(AuctionsApplicationInstaller).Assembly);
            services.AddEventSubscribers(typeof(AuctionsApplicationInstaller));
        }

        public AuctionsApplicationInstaller AddFileStreamAccessor(Func<IServiceProvider, IFileStreamAccessor> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public AuctionsApplicationInstaller AddTempFileService(Func<IServiceProvider, ITempFileService> factory)
        {
            Services.AddTransient(factory);
            return this;
        }



        public void Build() { }
    }
}
