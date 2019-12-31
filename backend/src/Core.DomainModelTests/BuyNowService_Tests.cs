using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Core.Common.Domain.Users.Events;
using Core.Common.DomainServices;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Core.DomainModelTests
{
    [TestFixture]
    public class BuyNowService_Tests
    {
        private Auction auction;
        private User[] users = new User[3];
        private BuyNowService service;

        [SetUp]
        public void SetUp()
        {
            auction = AuctionTestUtils.CreateAuction();
            for (int i = 0; i < users.Length; i++)
            {
                users[i] = AuctionTestUtils.CreateUser();
            }


            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.FindUser(It.IsAny<UserIdentity>())).Returns<UserIdentity>(identity =>
            {
                return auction.Bids.Where(bid => bid.UserIdentity.UserId.Equals(identity.UserId))
                    .Select(bid => bid.UserIdentity)
                    .Select(userIdentity =>
                        users.First(user => user.UserIdentity.UserId.Equals(userIdentity.UserId)))
                    .FirstOrDefault();
            });

            service = new BuyNowService(mockRepo.Object);
        }

        [Test]
        public void BuyNow_when_auction_have_bids_returns_credits_to_users_and_generates_valid_events()
        {
            auction.Raise(users[0], 1);
            var bid1 = auction.Bids.Last();

            auction.Raise(users[1], 2);
            var bid2 = auction.Bids.Last();

            auction.Raise(users[2], 3);
            var bid3 = auction.Bids.Last();

            auction.MarkPendingEventsAsHandled();
            foreach (var user in users)
            {
                user.MarkPendingEventsAsHandled();
            }

            var generatedEvents = service.BuyNow(auction, users[2]);
            generatedEvents.Count.Should().Be(3);


            foreach (var user in users.Take(2))
            {
                user.Credits.Should().Be(1000);
                user.PendingEvents.Count.Should().Be(1);
                user.PendingEvents.Last().Should().BeOfType<CreditsReturned>();
            }

            users[2].Credits.Should().Be(1000 - auction.BuyNowPrice);
            users[2].PendingEvents.Count.Should().Be(2);
            users[2].PendingEvents.First().Should().BeOfType<CreditsReturned>();
            users[2].PendingEvents.Last().Should().BeOfType<CreditsWithdrawn>();
        }
    }
}