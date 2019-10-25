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
                .SetProduct(new Product())
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
                .SetProduct(new Product())
                .SetCategory(new Category("test", 0))
                .SetBuyNowOnly(false)
                .Build();
            auction = new Auction(auctionArgs);
        }

        [Test]
        public void AuctionConstructor_when_valid_args_generates_valid_pending_events()
        {
            var image1 = new AuctionImage("1", "2", "3");
            var image2 = new AuctionImage("1", "2", "3");
            var imgs = new AuctionImage[] {image1, image2};
            var start = DateTime.UtcNow.AddMinutes(20);
            var end = DateTime.UtcNow.AddDays(1);
            var owner = new UserIdentity(){UserId = Guid.NewGuid(), UserName = "test"};

            var auctionArgs = new AuctionArgs.Builder()
                .SetBuyNow(90.00m)
                .SetStartDate(start)
                .SetEndDate(end)
                .SetOwner(owner)
                .SetProduct(new Product())
                .SetCategory(new Category("test", 0))
                .SetImages(imgs)
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
            auction.BuyNowPrice.Should()
                .Be(90.00m);
            auction.StartDate.Should()
                .Be(start);
            auction.EndDate.Should()
                .Be(end);
            auction.Owner.Should()
                .Be(owner);

            auction.PendingEvents.Count.Should()
                .Be(1);
            auction.PendingEvents.First()
                .Should()
                .BeOfType(typeof(AuctionCreated));
            auction.AggregateId.Should()
                .NotBeEmpty();
            var createdEvent = auction.PendingEvents.First() as AuctionCreated;
            createdEvent.AuctionArgs.Should().BeEquivalentTo(auctionArgs);
        }

        [TestCase(0, 0, true)]
        [TestCase(0, Auction.MIN_AUCTION_TIME_M - 1, true)]
        [TestCase(-Auction.MAX_TODAY_MIN_OFFSET, 0, true)]
        [TestCase(-Auction.MAX_TODAY_MIN_OFFSET + 1, 0, true)]
        [TestCase(-Auction.MAX_TODAY_MIN_OFFSET + 1, Auction.MIN_AUCTION_TIME_M - Auction.MAX_TODAY_MIN_OFFSET, true)]
        [TestCase(-Auction.MAX_TODAY_MIN_OFFSET + 1, Auction.MIN_AUCTION_TIME_M - Auction.MAX_TODAY_MIN_OFFSET + 1, false)]
        [TestCase(0, Auction.MIN_AUCTION_TIME_M, false)]
        public void AuctionConstructor_when_invalid_end_date_args_throws(int minutesStart, int minutesEnd, bool throws)
        {
            var date = DateTime.UtcNow;

            var auctionArgs = new AuctionArgs.Builder()
                .SetBuyNow(90.00m)
                .SetStartDate(date.AddMinutes(minutesStart))
                .SetEndDate(date.AddMinutes(minutesEnd))
                .SetOwner(new UserIdentity())
                .SetProduct(new Product())
                .SetCategory(new Category("test", 0))
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
                .SetProduct(new Product())
                .SetCategory(new Category("test", 0))
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
            auction.BuyNowPrice.Should()
                .Be(20);
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
            auction.StartDate.Should()
                .Be(start);
            auction.EndDate.Should()
                .Be(end);
        }

        [Test]
        public void Auction_FromEvents_builds_auction_from_pending_events()
        {
            auction.Raise(new Bid(auction.AggregateId, new UserIdentity()
            {
                UserId = Guid.NewGuid()
            }, 21));
            auction.EndAuction();

            var built = Auction.FromEvents(auction.PendingEvents);
            auction.MarkPendingEventsAsHandled();

            built.Should()
                .BeEquivalentTo(auction);
        }

        [Test]
        public void Raise_generates_valid_pending_events_and_state()
        {
            var bid = new Bid(auction.AggregateId, new UserIdentity(){UserId = Guid.NewGuid()}, 21);

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

        [TestCase(0)]
        [TestCase(10)]
        public void Raise_when_invalid_parameters_throws(decimal price)
        {
            auction.Raise(new Bid(auction.AggregateId, new UserIdentity(){UserId = Guid.NewGuid()}, 91));

            var bid1 = new Bid(auction.AggregateId, new UserIdentity(){UserId = Guid.NewGuid()}, price);

            Assert.Throws<DomainException>(() => auction.Raise(bid1));
        }

        [Test]
        public void Raise_when_buynowonly_throws()
        {
            var auction = CreateBuyNowOnlyAuction();

            var bid = new Bid(auction.AggregateId, new UserIdentity() {UserId = Guid.NewGuid()}, 12);
            Assert.Throws<DomainException>(() => auction.Raise(bid));
        }

        [Test]
        public void ChangeEndDate_when_valid_endDate_changes_EndDate()
        {
            var end = auction.EndDate;

            auction.ChangeEndDate(end.AddDays(12));

            auction.EndDate.Should()
                .Be(end.AddDays(12));
        }

        [Test]
        public void ChangeEndDate_when_invalid_endDate_throws()
        {
            Assert.Throws<DomainException>(() => auction.ChangeEndDate(auction.StartDate));
            Assert.Throws<DomainException>(() => auction.ChangeEndDate(auction.StartDate.AddDays(-1)));
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
            var bid = new Bid(auction.AggregateId, userIdnetity, 91);
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
                new UserIdentity(){UserId = Guid.NewGuid()}, 12)));
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
            for (int i = 1; i < Auction.MAX_IMAGES; i++)
            {
                auction.AddImage(image);
            }

            Assert.Throws<DomainException>(() => auction.AddImage(image));
        }
    }
}