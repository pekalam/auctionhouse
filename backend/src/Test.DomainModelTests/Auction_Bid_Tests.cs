using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Bids;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.Domain.Users.Events;
using Core.Common.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace Core.DomainModelTests
{
    [TestFixture]
    public class Auction_Bid_Tests
    {
        private Auction auction;

        [SetUp]
        public void SetUp()
        {
            var auctionArgs = new AuctionArgs.Builder()
                .SetBuyNow(90.00m)
                .SetStartDate(DateTime.UtcNow.AddMinutes(20))
                .SetEndDate(DateTime.UtcNow.AddDays(5))
                .SetOwner(new UserIdentity())
                .SetProduct(new Product("test name", "desccription 1111", Condition.New))
                .SetCategory(new Category("test", 0))
                .SetBuyNowOnly(false)
                .SetTags(new[] { "tag1" })
                .SetName("Test name")
                .Build();
            auction = new Auction(auctionArgs);
        }

        [Test]
        public void Raise_generates_valid_pending_events_and_state()
        {
            var user = AuctionTestUtils.CreateUser();

            auction.Raise(user, 21);

            auction.PendingEvents.Count.Should()
                .Be(2);
            var bid = auction.Bids.Last();
            var raisedEvent = auction.PendingEvents.Last() as AuctionRaised;
            raisedEvent.Should()
                .BeOfType(typeof(AuctionRaised));
            raisedEvent.Bid.Should()
                .Be(bid);
            auction.ActualPrice.Should()
                .Be(21);
            bid.UserIdentity.Should().BeEquivalentTo(user.UserIdentity);
            bid.AuctionId.Should().Be(auction.AggregateId);
            bid.DateCreated.Kind.Should().Be(DateTimeKind.Utc);

            user.Credits.Should().Be(1000 - bid.Price);
            user.PendingEvents.Should().HaveCount(1);
            user.PendingEvents.First().Should().BeOfType<CreditsWithdrawn>();
        }

        [TestCase(1)]
        [TestCase(10)]
        public void Raise_when_invalid_parameters_throws(decimal price)
        {
            var user = AuctionTestUtils.CreateUser();
            auction.Raise(user, 91);

            Assert.Throws<DomainException>(() => auction.Raise(user, price));
        }

        [Test]
        public void Raise_when_buynowonly_throws()
        {
            var user = AuctionTestUtils.CreateUser();
            var auction = AuctionTestUtils.CreateBuyNowOnlyAuction();

            Assert.Throws<DomainException>(() => auction.Raise(user, 12));
        }

        [Test]
        public void CancelBid_when_buynowOnly_throws()
        {
            var auction = AuctionTestUtils.CreateBuyNowOnlyAuction();
            var user = AuctionTestUtils.CreateUser();
            Assert.Throws<DomainException>(() => auction.CancelBid(user, new Bid(auction.AggregateId, user.UserIdentity, 12, DateTime.UtcNow)));
        }

        [Test]
        public void CancelBid_when_1_bid_exists_changes_currentPrice_to_0_and_returns_credits_to_user()
        {
            var user1 = AuctionTestUtils.CreateUser();
            auction.Raise(user1, 10);
            var firstBid = auction.Bids.Last();
            auction.MarkPendingEventsAsHandled();

            auction.CancelBid(user1, firstBid);

            auction.PendingEvents.Count.Should()
                .Be(1);
            auction.PendingEvents.First()
                .Should()
                .BeOfType<BidCanceled>();

            auction.ActualPrice.Should().Be(0);

            user1.PendingEvents.Should().HaveCount(2);
            user1.PendingEvents.First().Should().BeOfType<CreditsWithdrawn>();
            user1.PendingEvents.Last().Should().BeOfType<CreditsReturned>();
            user1.Credits.Should().Be(1000);
        }

        [Test]
        public void CancelBid_when_bid_exists_changes_currentPrice_and_returns_credits_to_user()
        {
            var user1 = AuctionTestUtils.CreateUser();
            var user2 = AuctionTestUtils.CreateUser();

            auction.Raise(user1, 10);
            var firstBid = auction.Bids.Last();
            auction.Raise(user2, 20);
            var secondBid = auction.Bids.Last();
            auction.MarkPendingEventsAsHandled();

            auction.CancelBid(user2, secondBid);

            auction.ActualPrice.Should()
                .Be(10);
            auction.Bids.First().Should().BeEquivalentTo(firstBid);
            auction.PendingEvents.Count.Should()
                .Be(1);
            auction.PendingEvents.First()
                .Should()
                .BeOfType<BidCanceled>();

            user2.PendingEvents.Should().HaveCount(2);
            user2.PendingEvents.First().Should().BeOfType<CreditsWithdrawn>();
            user2.PendingEvents.Last().Should().BeOfType<CreditsReturned>();
            user2.Credits.Should().Be(1000);
        }

        [Test]
        public void RemoveBid_when_bid_exists_removes_it()
        {
            var user1 = AuctionTestUtils.CreateUser();
            var user2 = AuctionTestUtils.CreateUser();

            auction.Raise(user1, 10);
            var firstBid = auction.Bids.Last();
            auction.Raise(user2, 20);
            var secondBid = auction.Bids.Last();
            auction.MarkPendingEventsAsHandled();

            auction.RemoveBid(firstBid);

            auction.ActualPrice.Should()
                .Be(20);
            auction.PendingEvents.Count.Should()
                .Be(1);
            auction.PendingEvents.First()
                .Should()
                .BeOfType<BidRemoved>();
        }

        [Test]
        public void Raise_when_user_does_not_have_enough_credits_throws()
        {
            var user = AuctionTestUtils.CreateUser(0);

            Assert.Throws<DomainException>(() => auction.Raise(user, 91));
        }

    }
}
