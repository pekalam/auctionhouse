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
    [Category("Integration")]
    public class UserRepository_Tests
    {
        private ESUserRepository userRepository;
        private User user;

        [SetUp]
        public void SetUp()
        {
            var esConnectionContext = new ESConnectionContext(new EventStoreConnectionSettings()
            {
                IPAddress = "192.168.1.25",
                Port = 1113
            });
            esConnectionContext.Connect();
            userRepository = new ESUserRepository(esConnectionContext);
            user = new User();
            user.Register("test");
            
        }

        [Test]
        public void Adduser_adds_user_and_Finduser_reads_it()
        {
            userRepository.AddUser(user);
            user.MarkPendingEventsAsHandled();

            var read = userRepository.FindUser(user.UserIdentity);

            read.Should().BeEquivalentTo(user);
        }


        [Test]
        public void FindUser_when_not_found_returns_null()
        {
            var read = userRepository.FindUser(user.UserIdentity);

            read.Should()
                .BeNull();
        }
    }



    [Category("Integration")]
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
                .Build();
            var auction = new Auction(auctionArgs);
            return auction;
        }

        [SetUp]
        public void SetUp()
        {
            var esConnectionContext = new ESConnectionContext(new EventStoreConnectionSettings()
            {
                IPAddress = "192.168.1.25",
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