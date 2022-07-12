using Adapter.Dapper.AuctionhouseDatabase;
using AuctionBids.Domain.Repositories;
using AuctionBids.Domain.Shared;
using FluentAssertions;
using System;
using TestConfigurationAccessor;
using Xunit;

namespace Test.Dapper.AuctionhouseDatabase
{
    public class AuctionBidsRepository_Tests
    {
        readonly IAuctionBidsRepository auctionBids;

        public AuctionBidsRepository_Tests()
        {
            var repositorySettings = TestConfig.Instance.GetRepositorySettings();
            auctionBids = new MsSqlAuctionBidsRepository(repositorySettings);
        }

        [Fact]
        public void Adds_auction_to_repository_and_finds_it_by_auctionId()
        {
            var bids = AuctionBids.Domain.AuctionBids.CreateNew(new AuctionId(Guid.NewGuid()), new AuctionCategoryIds(1, 2, 3), new UserId(Guid.NewGuid()));

            auctionBids.Add(bids);
            bids.MarkPendingEventsAsHandled();

            auctionBids.WithAuctionId(bids.AuctionId).Should().BeEquivalentTo(bids);
        }
    }
}
