using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Bids;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Moq;
using NUnit.Framework.Constraints;

namespace Core.DomainModelTests
{
    

    public class Auction_Tests
    {
        private Auction auction;

        [SetUp]
        public void SetUp()
        {
            auction = new Auction(90.00m, DateTime.UtcNow.AddMinutes(20), DateTime.UtcNow.AddDays(1),
                new UserIdentity(), new Product(), new Category("test", 0));
        }

        [Test]
        public void AuctionConstructor_when_valid_args_generates_valid_pending_events()
        {
            var image1 = new AuctionImage("1", "2", "3");
            var image2 = new AuctionImage("1", "2", "3");
            
            var auction = new Auction(90.00m, DateTime.UtcNow.AddMinutes(20), DateTime.UtcNow.AddDays(1),
                new UserIdentity(), new Product(), new Category("test", 0), new AuctionImage[]{image1, image2});

            auction.PendingEvents.Count.Should().Be(1);
            auction.PendingEvents.First().Should().BeOfType(typeof(AuctionCreated));
            auction.AggregateId.Should().NotBeEmpty();
            var createdEvent = auction.PendingEvents.First() as AuctionCreated;
            createdEvent.Category.Should().Be(auction.Category);
            createdEvent.AuctionImages.Length.Should().Be(Auction.MAX_IMAGES);
            createdEvent.AuctionImages[0].Should().Be(image1);
            createdEvent.AuctionImages[1].Should().Be(image2);
            for (int i = 2; i < createdEvent.AuctionImages.Length; i++)
            {
                createdEvent.AuctionImages[i].Should().BeNull();
            }
        }

        [Test]
        public void AuctionConstructor_when_invalid_end_date_args_throws()
        {
            Assert.Throws<DomainException>(() => new Auction(90.00m, DateTime.Today.AddDays(1), DateTime.Today,
                new UserIdentity(), new Product(), new Category("test", 0)));
        }

        [Test]
        public void Auction_FromEvents_builds_valid_auction()
        {
            var start = DateTime.UtcNow.AddMinutes(20);
            var end = start.AddDays(2);
            var id = Guid.NewGuid();
            var events = new Event[]
            {
                new AuctionCreated(id, new Product(), 20, start, end, new UserIdentity(), new Category("test", 0), null)
            };

            var auction = Auction.FromEvents(events);

            auction.PendingEvents.Count.Should().Be(0);
            auction.AggregateId.Should().NotBeEmpty().And.Be(id);
            auction.BuyNowPrice.Value.Should().Be(20);
            auction.Product.Should().NotBeNull();
            auction.Buyer.Should().Be(UserIdentity.Empty);
            auction.Completed.Should().BeFalse();
            auction.ActualPrice.Should().BeNull();
            auction.Owner.Should().NotBeNull();
            auction.StartDate.Should().Be(start);
            auction.EndDate.Should().Be(end);
        }

        [Test]
        public void Auction_FromEvents_builds_auction_from_pending_events()
        {
            auction.Raise(new Bid(auction.AggregateId, new UserIdentity(), 21));
            auction.EndAuction();

            var built = Auction.FromEvents(auction.PendingEvents);
            auction.MarkPendingEventsAsHandled();

            built.Should().BeEquivalentTo(auction);
        }

        [Test]
        public void Raise_generates_valid_pending_events_and_state()
        {
            var bid = new Bid(auction.AggregateId, new UserIdentity(), 21);

            auction.Raise(bid);

            auction.PendingEvents.Count.Should().Be(2);
            var raisedEvent = auction.PendingEvents.Last() as AuctionRaised;
            raisedEvent.Should().BeOfType(typeof(AuctionRaised));
            raisedEvent.Bid.Should().Be(bid);
            auction.ActualPrice.Should().Be(bid.Price);
        }

        [TestCase(0)]
        [TestCase(10)]
        public void Raise_when_invalid_parameters_throws(decimal price)
        {
            auction.Raise(new Bid(auction.AggregateId, new UserIdentity(), 91));

            var bid1 = new Bid(auction.AggregateId, new UserIdentity(), price);

            Assert.Throws<DomainException>(() => auction.Raise(bid1));
        }

        [Test]
        public void ChangeEndDate_when_valid_endDate_changes_EndDate()
        {
            var end = auction.EndDate;

            auction.ChangeEndDate(end.AddDays(12));

            auction.EndDate.Should().Be(end.AddDays(12));
        }

        [Test]
        public void EndAuction_when_no_bids_generates_valid_event_and_state()
        {
            auction.MarkPendingEventsAsHandled();
            
            auction.EndAuction();

            auction.PendingEvents.Count.Should().Be(1);
            auction.Buyer.Should().BeNull();
            auction.Completed.Should().BeTrue();

            var ev = auction.PendingEvents.First() as AuctionCompleted;

            ev.AuctionId.Should().Be(auction.AggregateId);
            ev.WinningBid.Should().BeNull();
        }

        [Test]
        public void EndAuction_when_has_bids_generates_valid_event_and_state()
        {
            var userIdnetity = new UserIdentity();
            var bid = new Bid(auction.AggregateId, userIdnetity, 91);
            auction.Raise(bid);
            auction.MarkPendingEventsAsHandled();

            auction.EndAuction();

            auction.PendingEvents.Count.Should().Be(1);
            auction.Buyer.Should().Be(userIdnetity);
            auction.Completed.Should().BeTrue();


            var ev = auction.PendingEvents.First() as AuctionCompleted;

            ev.AuctionId.Should().Be(auction.AggregateId);
            ev.WinningBid.Should().Be(bid);
        }

        [Test]
        public void CancelAuction_when_incompleted_generates_valid_events_and_state()
        {
            auction.MarkPendingEventsAsHandled();

            auction.CancelAuction();

            auction.PendingEvents.Count.Should().Be(1);
            var ev = auction.PendingEvents.First() as AuctionCanceled;
            ev.AuctionId.Should().Be(auction.AggregateId);
        }

        [Test]
        public void CancelAuction_when_completed_throws()
        {
            auction.EndAuction();
            auction.MarkPendingEventsAsHandled();

            Assert.Throws<DomainException>(() => auction.CancelAuction());
        }

        [Test]
        public void BuyNow_generates_valid_events_and_state()
        {
            var userIdnetity = new UserIdentity();
            auction.MarkPendingEventsAsHandled();

            auction.BuyNow(userIdnetity);

            auction.PendingEvents.Count.Should().Be(1);
            auction.Buyer.Should().Be(userIdnetity);
            auction.Completed.Should().BeTrue();
            var ev = auction.PendingEvents.First() as AuctionBought;

            ev.AuctionId.Should().Be(auction.AggregateId);
            ev.UserIdentity.Should().Be(userIdnetity);
        }

        [Test]
        public void AddImage_when_empty_image_list_adds_image()
        {
            var image = new AuctionImage("id1", "id2", "id3");
            auction.MarkPendingEventsAsHandled();
            auction.AddImage(image);

            var ev = auction.PendingEvents.First() as AuctionImageAdded;

            auction.PendingEvents.Count.Should().Be(1);
            auction.PendingEvents.First().GetType().Should().Be(typeof(AuctionImageAdded));
            ev.AddedImage.Should().Be(image);
            ev.AuctionId.Should().Be(auction.AggregateId);
            ev.Num.Should().Be(0);
        }

        [Test]
        public void AddImage_when_full_image_list_throws()
        {
            var auction = new Auction(90.00m, DateTime.UtcNow.AddMinutes(20), DateTime.UtcNow.AddDays(1),
                new UserIdentity(), new Product(), new Category("test", 0));
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