using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Bids;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.Exceptions;

namespace Core.DomainModelTests
{
    public class Auction_Tests
    {
        private Auction auction;


        private Auction CreateBuyNowOnlyAuction()
        {
            var args = new AuctionArgs.Builder()
                .SetBuyNowOnly(true)
                .SetOwner(new UserIdentity())
                .SetCategory(new Category("", 1))
                .SetBuyNow(123)
                .SetStartDate(DateTime.UtcNow.AddDays(1))
                .SetEndDate(DateTime.UtcNow.AddDays(2))
                .SetProduct(new Product("test name", "desccription 1111", Condition.New))
                .SetTags(new[] {"tag1"})
                .SetName("Test name")
                .Build();
            return new Auction(args);
        }

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
                .SetTags(new[] {"tag1"})
                .SetName("Test name")
                .Build();
            auction = new Auction(auctionArgs);
        }

        [Test]
        public void AuctionConstructor_when_invalid_tags_throws()
        {
            var builder = new AuctionArgs.Builder()
                .SetBuyNow(90.00m)
                .SetStartDate(DateTime.UtcNow.AddMinutes(20))
                .SetEndDate(DateTime.UtcNow.AddDays(5))
                .SetOwner(new UserIdentity())
                .SetProduct(new Product("test name", "desccription 1111", Condition.New))
                .SetCategory(new Category("test", 0))
                .SetBuyNowOnly(false)
                .SetName("Test name");
            var args = builder
                .SetTags(Enumerable.Range(0, Auction.MIN_TAGS - 1)
                    .Select(i => $"tag1{i}")
                    .ToArray())
                .Build();
            Assert.Throws<DomainException>(() => new Auction(args));
        }

        [Test]
        public void AuctionConstructor_when_valid_args_generates_valid_pending_events()
        {
            var image1 = new AuctionImage("1", "2", "3");
            var image2 = new AuctionImage("1", "2", "3");
            var imgs = new AuctionImage[] {image1, image2};
            var start = DateTime.UtcNow.AddMinutes(20);
            var end = DateTime.UtcNow.AddDays(1);
            var owner = new UserIdentity() {UserId = Guid.NewGuid(), UserName = "test"};

            var auctionArgs = new AuctionArgs.Builder()
                .SetBuyNow(90.00m)
                .SetStartDate(start)
                .SetEndDate(end)
                .SetOwner(owner)
                .SetProduct(new Product("test name", "desccription 1111", Condition.New))
                .SetCategory(new Category("test", 0))
                .SetImages(imgs)
                .SetTags(new[] {"tag1"})
                .SetName("Test auction")
                .Build();
            var auction = new Auction(auctionArgs);

            auction.Category.Should()
                .Be(auctionArgs.Category);
            auction.ActualPrice.Should()
                .Be(0);
            auction.AuctionImages[0]
                .Should()
                .Be(imgs[0]);
            auction.AuctionImages[1]
                .Should()
                .Be(imgs[1]);
            auction.Bids.Count.Should()
                .Be(0);
            auction.BuyNowPrice.Value.Should()
                .Be(90.00m);
            auction.StartDate.Value.Should()
                .Be(start);
            auction.EndDate.Value.Should()
                .Be(end);
            auction.Owner.Should()
                .Be(owner);
            auction.Tags[0]
                .Value
                .Should()
                .Be("tag1");
            auction.Tags.Length.Should()
                .Be(1);
            auction.Name.Value.Should()
                .Be("Test auction");

            auction.PendingEvents.Count.Should()
                .Be(1);
            auction.AggregateId.Should()
                .NotBeEmpty();
            var createdEvent = auction.PendingEvents.First() as AuctionCreated;
            createdEvent.Should()
                .NotBeNull();
            createdEvent.AuctionArgs.Should()
                .BeEquivalentTo(auctionArgs);
        }

        [TestCase(0, 0, true)]
        [TestCase(0, AuctionConstantsFactory.DEFAULT_MIN_AUCTION_TIME_M - 1, true)]
        [TestCase(-AuctionConstantsFactory.DEFAULT_MAX_TODAY_MIN_OFFSET, 0, true)]
        [TestCase(-AuctionConstantsFactory.DEFAULT_MAX_TODAY_MIN_OFFSET + 1, 0, true)]
        [TestCase(-AuctionConstantsFactory.DEFAULT_MAX_TODAY_MIN_OFFSET + 1,
            AuctionConstantsFactory.DEFAULT_MIN_AUCTION_TIME_M - AuctionConstantsFactory.DEFAULT_MAX_TODAY_MIN_OFFSET,
            true)]
        [TestCase(-AuctionConstantsFactory.DEFAULT_MAX_TODAY_MIN_OFFSET + 1,
            AuctionConstantsFactory.DEFAULT_MIN_AUCTION_TIME_M - AuctionConstantsFactory.DEFAULT_MAX_TODAY_MIN_OFFSET +
            1, false)]
        [TestCase(0, AuctionConstantsFactory.DEFAULT_MIN_AUCTION_TIME_M, false)]
        public void AuctionConstructor_when_invalid_end_date_args_throws(int minutesStart, int minutesEnd, bool throws)
        {
            var date = DateTime.UtcNow;

            var auctionArgs = new AuctionArgs.Builder()
                .SetBuyNow(90.00m)
                .SetStartDate(date.AddMinutes(minutesStart))
                .SetEndDate(date.AddMinutes(minutesEnd))
                .SetOwner(new UserIdentity())
                .SetProduct(new Product("test name", "desccription 1111", Condition.New))
                .SetCategory(new Category("test", 0))
                .SetTags(new[] {"t1"})
                .SetName("test name")
                .Build();
            if (throws)
            {
                Assert.Throws<DomainException>(() => new Auction(auctionArgs));
            }
            else
            {
                Assert.DoesNotThrow(() => new Auction(auctionArgs));
            }
        }

        [Test]
        public void Auction_FromEvents_builds_valid_auction()
        {
            var start = DateTime.UtcNow.AddMinutes(20);
            var end = start.AddDays(2);
            var id = Guid.NewGuid();
            var auctionArgs = new AuctionArgs.Builder()
                .SetBuyNow(20)
                .SetStartDate(start)
                .SetEndDate(end)
                .SetOwner(new UserIdentity())
                .SetProduct(new Product("test name", "desccription 1111", Condition.New))
                .SetCategory(new Category("test", 0))
                .SetTags(new[] {"tag1"})
                .SetName("Test name")
                .Build();
            var events = new Event[]
            {
                new AuctionCreated(id, auctionArgs)
            };

            var auction = Auction.FromEvents(events);

            auction.PendingEvents.Count.Should()
                .Be(0);
            auction.AggregateId.Should()
                .NotBeEmpty()
                .And.Be(id);
            auction.BuyNowPrice.Value.Should()
                .Be(20m);
            auction.Product.Should()
                .NotBeNull();
            auction.Buyer.Should()
                .Be(UserIdentity.Empty);
            auction.Completed.Should()
                .BeFalse();
            auction.ActualPrice.Should()
                .Be(0);
            auction.Owner.Should()
                .NotBeNull();
            auction.StartDate.Value.Should()
                .Be(start);
            auction.EndDate.Value.Should()
                .Be(end);
            auction.Tags.Length.Should()
                .Be(1);
            auction.Tags[0]
                .Value.Should()
                .Be("tag1");
        }

        [Test]
        public void Auction_FromEvents_builds_auction_from_pending_events()
        {
            auction.Raise(new Bid(auction.AggregateId, new UserIdentity()
            {
                UserId = Guid.NewGuid()
            }, 21, DateTime.UtcNow));
            auction.EndAuction();

            var built = Auction.FromEvents(auction.PendingEvents);
            auction.MarkPendingEventsAsHandled();

            built.Should()
                .BeEquivalentTo(auction);
        }

        [Test]
        public void Raise_generates_valid_pending_events_and_state()
        {
            var bid = new Bid(auction.AggregateId, new UserIdentity() {UserId = Guid.NewGuid()}, 21, DateTime.UtcNow);

            auction.Raise(bid);

            auction.PendingEvents.Count.Should()
                .Be(2);
            var raisedEvent = auction.PendingEvents.Last() as AuctionRaised;
            raisedEvent.Should()
                .BeOfType(typeof(AuctionRaised));
            raisedEvent.Bid.Should()
                .Be(bid);
            auction.ActualPrice.Should()
                .Be(bid.Price);
        }

        [TestCase(1)]
        [TestCase(10)]
        public void Raise_when_invalid_parameters_throws(decimal price)
        {
            auction.Raise(new Bid(auction.AggregateId, new UserIdentity() {UserId = Guid.NewGuid()}, 91,
                DateTime.UtcNow));

            var bid1 = new Bid(auction.AggregateId, new UserIdentity() {UserId = Guid.NewGuid()}, price,
                DateTime.UtcNow);

            Assert.Throws<DomainException>(() => auction.Raise(bid1));
        }

        [Test]
        public void Raise_when_buynowonly_throws()
        {
            var auction = CreateBuyNowOnlyAuction();

            var bid = new Bid(auction.AggregateId, new UserIdentity() {UserId = Guid.NewGuid()}, 12, DateTime.UtcNow);
            Assert.Throws<DomainException>(() => auction.Raise(bid));
        }

        [Test]
        public void ChangeEndDate_when_valid_endDate_changes_EndDate()
        {
            var end = auction.EndDate;

            auction.UpdateEndDate(end.Value.AddDays(12));

            auction.EndDate.Value.Should()
                .Be(end.Value.AddDays(12));
        }

        [Test]
        public void ChangeEndDate_when_invalid_endDate_throws()
        {
            Assert.Throws<DomainException>(() => auction.UpdateEndDate(auction.StartDate));
            Assert.Throws<DomainException>(() => auction.UpdateEndDate(auction.StartDate.Value.AddDays(-1)));
        }

        [Test]
        public void EndAuction_when_no_bids_generates_valid_event_and_state()
        {
            auction.MarkPendingEventsAsHandled();

            auction.EndAuction();

            auction.PendingEvents.Count.Should()
                .Be(1);
            auction.Buyer.Should()
                .BeNull();
            auction.Completed.Should()
                .BeTrue();

            var ev = auction.PendingEvents.First() as AuctionCompleted;

            ev.AuctionId.Should()
                .Be(auction.AggregateId);
            ev.WinningBid.Should()
                .BeNull();
        }

        [Test]
        public void EndAuction_when_has_bids_generates_valid_event_and_state()
        {
            var userIdnetity = new UserIdentity()
            {
                UserId = Guid.NewGuid()
            };
            var bid = new Bid(auction.AggregateId, userIdnetity, 91, DateTime.UtcNow);
            auction.Raise(bid);
            auction.MarkPendingEventsAsHandled();

            auction.EndAuction();

            auction.PendingEvents.Count.Should()
                .Be(1);
            auction.Buyer.Should()
                .Be(userIdnetity);
            auction.Completed.Should()
                .BeTrue();


            var ev = auction.PendingEvents.First() as AuctionCompleted;

            ev.AuctionId.Should()
                .Be(auction.AggregateId);
            ev.WinningBid.Should()
                .Be(bid);
        }

        [Test]
        public void CancelAuction_when_incompleted_generates_valid_events_and_state()
        {
            auction.MarkPendingEventsAsHandled();

            auction.CancelAuction();

            auction.PendingEvents.Count.Should()
                .Be(1);
            var ev = auction.PendingEvents.First() as AuctionCanceled;
            ev.AuctionId.Should()
                .Be(auction.AggregateId);
        }

        [Test]
        public void CancelAuction_when_completed_throws()
        {
            auction.EndAuction();
            auction.MarkPendingEventsAsHandled();

            Assert.Throws<DomainException>(() => auction.CancelAuction());
        }

        [Test]
        public void CancelBid_when_buynowOnly_throws()
        {
            var auction = CreateBuyNowOnlyAuction();
            Assert.Throws<DomainException>(() => auction.CancelBid(new Bid(auction.AggregateId,
                new UserIdentity() {UserId = Guid.NewGuid()}, 12, DateTime.UtcNow)));
        }

        [Test]
        public void CancelBid_when_bid_exists_changes_currentPrice()
        {
            var firstBid = new Bid(auction.AggregateId, new UserIdentity() {UserId = Guid.NewGuid(), UserName = "test"},
                10, DateTime.UtcNow);
            var secondBid = new Bid(auction.AggregateId,
                new UserIdentity() {UserId = Guid.NewGuid(), UserName = "test2"}, 20, DateTime.UtcNow);

            auction.Raise(firstBid);
            auction.Raise(secondBid);
            auction.MarkPendingEventsAsHandled();

            auction.CancelBid(secondBid);

            auction.ActualPrice.Should()
                .Be(10);
            auction.Bids.First().Should().BeEquivalentTo(firstBid);
            auction.PendingEvents.Count.Should()
                .Be(2);
            auction.PendingEvents.First()
                .Should()
                .BeOfType<BidCanceled>();
            auction.PendingEvents.ElementAt(1)
                .Should()
                .BeOfType<AuctionRaised>();
        }

        [Test]
        public void BuyNow_generates_valid_events_and_state()
        {
            var userIdnetity = new UserIdentity();
            auction.MarkPendingEventsAsHandled();

            auction.BuyNow(userIdnetity);

            auction.PendingEvents.Count.Should()
                .Be(1);
            auction.Buyer.Should()
                .Be(userIdnetity);
            auction.Completed.Should()
                .BeTrue();
            var ev = auction.PendingEvents.First() as AuctionBought;

            ev.AuctionId.Should()
                .Be(auction.AggregateId);
            ev.UserIdentity.Should()
                .Be(userIdnetity);
        }

        [Test]
        public void AddImage_when_empty_image_list_adds_image()
        {
            var image = new AuctionImage("id1", "id2", "id3");
            auction.MarkPendingEventsAsHandled();
            auction.AddImage(image);

            var ev = auction.PendingEvents.First() as AuctionImageAdded;

            auction.PendingEvents.Count.Should()
                .Be(1);
            auction.PendingEvents.First()
                .GetType()
                .Should()
                .Be(typeof(AuctionImageAdded));
            ev.AddedImage.Should()
                .Be(image);
            ev.AuctionId.Should()
                .Be(auction.AggregateId);
            ev.Num.Should()
                .Be(0);
        }

        [Test]
        public void AddImage_when_full_image_list_throws()
        {
            var image = new AuctionImage("id1", "id2", "id3");
            auction.MarkPendingEventsAsHandled();
            for (int i = 1; i <= Auction.MAX_IMAGES; i++)
            {
                auction.AddImage(image);
            }

            Assert.Throws<DomainException>(() => auction.AddImage(image));
        }

        [Test]
        public void RemoveBid_when_bid_exists_removes()
        {
            var firstBid = new Bid(auction.AggregateId, new UserIdentity() { UserId = Guid.NewGuid(), UserName = "test" },
                10, DateTime.UtcNow);
            var secondBid = new Bid(auction.AggregateId,
                new UserIdentity() { UserId = Guid.NewGuid(), UserName = "test2" }, 20, DateTime.UtcNow);

            auction.Raise(firstBid);
            auction.Raise(secondBid);
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
        public void When_built_from_events_containing_update_event_group_builds_valid_object()
        {
            auction.UpdateBuyNowPrice(300m);
            auction.UpdateTags(new Tag[]{new Tag("update test1"), new Tag("update test2")});
            auction.UpdateCategory(new Category("update test category", 1));

            var recreated = Auction.FromEvents(auction.PendingEvents);

            Assert.IsTrue(recreated.BuyNowPrice == 300m);
            recreated.AggregateId.Should().Be(auction.AggregateId);
            recreated.Tags[0].Value.Should().Be("update test1");
            recreated.Tags[1].Value.Should().Be("update test2");
            recreated.Category.Name.Should().Be("update test category");
        }
    }
}