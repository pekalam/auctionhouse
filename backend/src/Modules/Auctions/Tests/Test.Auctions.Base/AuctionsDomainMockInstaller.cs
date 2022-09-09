using Auctions.Application;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Core.Common;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Auctions.Tests.Base
{
    public class AuctionsDomainMockInstaller : AuctionsDomainInstaller
    {
        public AuctionsDomainMockInstaller(IServiceCollection services) : base(services)
        {
            AddAuctionRepository(_ => Mock.Of<IAuctionRepository>())
            .AddAuctionCreateSessionStore(_ => Mock.Of<IAuctionCreateSessionStore>())
            .AddAuctionImageConversion(_ => Mock.Of<IAuctionImageConversion>())
            .AddAuctionImageRepository(_ => Mock.Of<IAuctionImageRepository>())
            .AddCategoryNamesToTreeIdsConversion(_ => Mock.Of<ICategoryNamesToTreeIdsConversion>())
            .AddAuctionUnlockScheduler(_ => Mock.Of<IAuctionUnlockScheduler>())
            .AddAuctionEndScheduler(_ => Mock.Of<IAuctionEndScheduler>())
            .AddAuctionPaymentVerification(_ => Mock.Of<IAuctionPaymentVerification>());
        }
    }

    public class AuctionsApplicationMockInstaller : AuctionsApplicationInstaller
    {
        public AuctionsApplicationMockInstaller(IServiceCollection services) : base(services)
        {
            AddFileStreamAccessor(_ => Mock.Of<IFileStreamAccessor>())
                .AddTempFileService(_ => Mock.Of<ITempFileService>());
        }
    }

    public class AuctionsMockInstaller
    {
        private readonly AuctionsDomainMockInstaller _domainInstaller;
        private readonly AuctionsApplicationMockInstaller _applicationInstaller;

        public AuctionsMockInstaller(IServiceCollection services)
        {
            _domainInstaller = new(services);
            _applicationInstaller = new(services);
        }

        public AuctionsDomainMockInstaller Domain => _domainInstaller;
        public AuctionsApplicationMockInstaller Application => _applicationInstaller;
    }
}
