using System;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using FluentAssertions;
using Infrastructure.Adapters.Repositories.EventStore;
using NUnit.Framework;

namespace Infrastructure.Tests.Integration
{
    [Category("Integration")]
    public class AuctionRepository_Tests
    {
        private ESAuctionRepository auctionRepository;


        private Auction CreateFakeAuction()
        {
            var auction = new Auction(20.0m, DateTime.UtcNow.AddMinutes(10), DateTime.UtcNow.AddDays(1),
                new UserIdentity() { UserName = "test", UserId = Guid.NewGuid() }, new Product()
                {
                    Name = "test product",
                    Description = "description"
                }, new Category("test", 0));
            return auction;
        }

        [SetUp]
        public void SetUp()
        {
            var esConnectionContext = new ESConnectionContext(new EventStoreConnectionSettings()
            {
                IPAddress = "127.0.0.1",
                Port = 1113
            });
            esConnectionContext.Connect();
            auctionRepository = new ESAuctionRepository(esConnectionContext);
        }

        [Test]
        public void AddAuction_adds_auction_and_FindAuction_reads_it()
        {
            var auction = CreateFakeAuction();
            auctionRepository.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            var read = auctionRepository.FindAuction(auction.AggregateId);

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
    }
}