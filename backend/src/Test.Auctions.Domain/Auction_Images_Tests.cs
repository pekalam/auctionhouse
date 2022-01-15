using Auctions.Domain;
using Auctions.DomainEvents;
using Core.DomainFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Auctions.Base.Builders;
using Xunit;

namespace Test.AuctionsDomain
{
    [Trait("Category", "Unit")]
    public class Auction_Images_Tests
    {
        [Fact]
        public void Adds_image_to_auction_images_list()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var image = new GivenAuctionImage().Valid();
            auction.MarkPendingEventsAsHandled();
            auction.AddImage(image);

            var ev = auction.PendingEvents.First() as AuctionImageAdded;

            auction.PendingEvents.Count.Should()
                .Be(1);
            auction.PendingEvents.First()
                .GetType()
                .Should()
                .Be(typeof(AuctionImageAdded));
            ev.AddedImageSize1Id.Should()
                .Be(image.Size1Id);
            ev.AddedImageSize2Id.Should()
                .Be(image.Size2Id);
            ev.AddedImageSize3Id.Should()
                .Be(image.Size3Id);
            ev.AuctionId.Should()
                .Be(auction.AggregateId);
            ev.Num.Should()
                .Be(0);
        }


        [Fact]
        public void AddImage_when_full_image_list_throws()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var image = new GivenAuctionImage().Valid();
            auction.MarkPendingEventsAsHandled();
            for (int i = 1; i <= Auction.MAX_IMAGES; i++)
            {
                auction.AddImage(image);
            }

            Assert.Throws<DomainException>(() => auction.AddImage(image));
        }

        [Fact]
        public void ChangeEndDate_when_invalid_endDate_throws()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            Assert.Throws<DomainException>(() => auction.UpdateEndDate(auction.StartDate));
            Assert.Throws<DomainException>(() => auction.UpdateEndDate(auction.StartDate.Value.AddDays(-1)));
        }

        [Fact]
        public void ChangeEndDate_when_valid_endDate_changes_EndDate()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var end = auction.EndDate;

            auction.UpdateEndDate(end.Value.AddDays(12));

            auction.EndDate.Value.Should()
                .Be(end.Value.AddDays(12));
        }
    }
}
