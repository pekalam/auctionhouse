using System;
using Core.Common.Domain.Auctions;
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
        private User user;

        private Auction CreateFakeAuction()
        {
            var auctionArgs = new AuctionArgs.Builder()
                .SetBuyNow(20.0m)
                .SetStartDate(DateTime.UtcNow.AddMinutes(10))
                .SetEndDate(DateTime.UtcNow.AddDays(1))
                .SetOwner(Core.Common.Domain.Auctions.UserId.New())
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
                    "Data Source=.;Initial Catalog=AuctionhouseDatabase;Integrated Security=False;User ID=sa;PWD=Qwerty1234;")
            };
            auctionRepository = new MsSqlAuctionRepository(serverOpt);
            user = User.Create(new Username("Test username"));
            user.AddCredits(1000);
            user.MarkPendingEventsAsHandled();
        }

        [Test]
        public void AddAuction_adds_auction_and_FindAuction_reads_it()
        {
            var auction = CreateFakeAuction();

            //auction.Raise(user, 10); //TODO

            auctionRepository.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            var read = auctionRepository.FindAuction(auction.AggregateId);

            read.Should().BeEquivalentTo(auction);
        }

        [Test]
        public void AddAuction_adds_auction_UpdateAuction_updates_and_FindAuction_finds_it_by_version()
        {
            var auction = CreateFakeAuction();
            //auction.Raise(user, 10); //TODO

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

        [Test]
        public void UpdateAuction_updates_auction_with_1_event_and_find_finds_it()
        {
            var auction = CreateFakeAuction();
            auctionRepository.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();
            auction.UpdateDescription("new description");

            auctionRepository.UpdateAuction(auction);
            auction.MarkPendingEventsAsHandled();

            var read = auctionRepository.FindAuction(auction.AggregateId);

            read.Should().BeEquivalentTo(auction);
        }

        [Test]
        public void UpdateAuction_updates_auction_with_more_than_1_event_and_find_finds_it()
        {
            var auction = CreateFakeAuction();
            auction.UpdateDescription("new description");

            auctionRepository.UpdateAuction(auction);
            auction.MarkPendingEventsAsHandled();

            var read = auctionRepository.FindAuction(auction.AggregateId);

            read.Should().BeEquivalentTo(auction);
        }
    }
}