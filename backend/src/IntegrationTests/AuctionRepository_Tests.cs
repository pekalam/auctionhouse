using System;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using FluentAssertions;
using Infrastructure.Repositories.EventStore;
using NUnit.Framework;

namespace IntegrationTests
{
    public class AuctionRepository_Tests
    {
        private ESAuctionRepository auctionRepository;


        private Auction CreateFakeAuction()
        {
            var auctionArgs = new AuctionArgs.Builder()
                .SetBuyNow(20.0m)
                .SetStartDate(DateTime.UtcNow.AddMinutes(10))
                .SetEndDate(DateTime.UtcNow.AddDays(1))
                .SetOwner(new UserIdentity() { UserName = "test", UserId = Guid.NewGuid() })
                .SetProduct(new Product()
                {
                    Name = "test product",
                    Description = "description"
                })
                .SetCategory(new Category("test", 0))
                .SetTags(new []{"tag1", "tag2"})
                .Build();
            var auction = new Auction(auctionArgs);
            return auction;
        }

        [SetUp]
        public void SetUp()
        {
            var esConnectionContext = new ESConnectionContext(new EventStoreConnectionSettings()
            {
                IPAddress = TestContextUtils.GetParameterOrDefault("eventstore-connection-string", "localhost"),
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

        [Test]
        public void FindAuction_when_not_found_returns_null()
        {
            var read = auctionRepository.FindAuction(Guid.NewGuid());

            read.Should()
                .BeNull();
        }
    }
}