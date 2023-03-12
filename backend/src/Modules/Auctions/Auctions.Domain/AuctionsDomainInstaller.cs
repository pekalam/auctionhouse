using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Domain
{
    public class AuctionsDomainInstaller
    {
        public AuctionsDomainInstaller(IServiceCollection services)
        {
            Services = services;
            AddCoreServices(services);
        }
        
        public IServiceCollection Services { get; }

        private void AddCoreServices(IServiceCollection services)
        {
            services.AddTransient<AuctionImageService>();
            services.AddTransient<AuctionBuyCancellationService>();
        }

        public AuctionsDomainInstaller AddAuctionRepository(Func<IServiceProvider, IAuctionRepository> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public AuctionsDomainInstaller AddAuctionImageRepository(Func<IServiceProvider, IAuctionImageRepository> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public AuctionsDomainInstaller AddAuctionCreateSessionStore(Func<IServiceProvider, IAuctionCreateSessionStore> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public AuctionsDomainInstaller AddAuctionEndScheduler(Func<IServiceProvider, IAuctionEndScheduler> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public AuctionsDomainInstaller AddAuctionImageConversion(Func<IServiceProvider, IAuctionImageConversion> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public AuctionsDomainInstaller AddAuctionPaymentVerification(Func<IServiceProvider, IAuctionPaymentVerification> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public AuctionsDomainInstaller AddAuctionCancelScheduler(Func<IServiceProvider, IAuctionBuyCancellationScheduler> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public AuctionsDomainInstaller AddCategoryNamesToTreeIdsConversion(Func<IServiceProvider, ICategoryNamesToTreeIdsConversion> factory)
        {
            Services.AddTransient(factory);
            return this;
        }
    }
}
