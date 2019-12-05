using System;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Bids;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using FluentAssertions;
using Infrastructure.Repositories.SQLServer;
using NUnit.Framework;

namespace IntegrationTests
{
    public class AuctionRepository_Tests
    {
        private IAuctionRepository auctionRepository;


        private Auction CreateFakeAuction()
        {
            var auctionArgs = new AuctionArgs.Builder()
                .SetBuyNow(20.0m)
                .SetStartDate(DateTime.UtcNow.AddMinutes(10))
                .SetEndDate(DateTime.UtcNow.AddDays(1))
                .SetOwner(new UserIdentity() { UserName = "test", UserId = Guid.NewGuid() })
                .SetProduct(new Product("product name", "description 1111", Condition.New))
                .SetCategory(new Category("test", 0))
                .SetTags(new []{"tag1", "tag2"})
                .SetName("Test name")
                .Build();
            var auction = new Auction(auctionArgs);
            return auction;
        }

        [SetUp]
        public void SetUp()
        {
            var serverOpt = new MsSqlConnectionSettings()
            {
                ConnectionString = TestContextUtils.GetParameterOrDefault("sqlserver", 
                    "Data Source=.;Initial Catalog=es;Integrated Security=True;")
            };
            auctionRepository = new MsSqlAuctionRepository(serverOpt);
        }

        [Test]
        public void AddAuction_adds_auction_and_FindAuction_reads_it()
        {
            var auction = CreateFakeAuction();
            var bid = new Bid(auction.AggregateId, new UserIdentity(Guid.NewGuid(), "test2"), 10, DateTime.UtcNow);
            auction.Raise(bid);

            auctionRepository.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            var read = auctionRepository.FindAuction(auction.AggregateId);

            read.Should().BeEquivalentTo(auction);
        }

        [Test]
        public void AddAuction_adds_auction_UpdateAuction_updates_and_FindAuction_finds_it_by_version()
        {
            var auction = CreateFakeAuction();
            var bid = new Bid(auction.AggregateId, new UserIdentity(Guid.NewGuid(), "test2"), 10, DateTime.UtcNow);
            auction.Raise(bid);

            auctionRepository.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            auction.UpdateDescription("New description");

            auctionRepository.UpdateAuction(auction);
            auction.MarkPendingEventsAsHandled();

            var read = auctionRepository.FindAuction(auction.AggregateId, auction.Version);

            read.Should().BeEquivalentTo(auction);
        }


        [Test]
        public void RemoveAuction_removes_added_auction()
        {
            var auction = CreateFakeAuction();
            auctionRepository.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();
            auctionRepository.RemoveAuction(auction.AggregateId);

            var read = auctionRepository.FindAuction(auction.AggregateId);

            read.Should().BeNull();
        }

        [Test]
        public void FindAuction_when_not_found_returns_null()
        {
            var read = auctionRepository.FindAuction(Guid.NewGuid());

            read.Should()
                .BeNull();
        }
    }
}