using Auctions.DomainEvents;
using Auctions.Tests.Base.Domain.ModelBuilders;
using Core.DomainFramework;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Auctions.Domain.Tests
{
    public class Auction_Images_Tests
    {
        [Fact]
        public void Adds_image_to_auction_images_list()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            AuctionImage image = new GivenAuctionImage().Build();
            auction.MarkPendingEventsAsHandled();
            auction.AddImage(image);

            var ev = (AuctionImageAdded)auction.PendingEvents.First();
            AuctionImageAddedEventShouldBeValid(auction, image, ev);
            auction.PendingEvents.Count.Should().Be(1);
        }

        private static void AuctionImageAddedEventShouldBeValid(Auction auction, AuctionImage image, AuctionImageAdded ev)
        {
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
        public void Fails_when_trying_to_add_to_full_image_list()
        {
            var auction = GivenValidAuction();
            var image = new GivenAuctionImage().Build();
            var addAuctionImageAction = () => auction.AddImage(image);

            for (var i = 1; i <= Auction.MAX_IMAGES; i++)
            {
                addAuctionImageAction();
            }

            addAuctionImageAction.Should().Throw<DomainException>().WithMessage("Could not add auction image");
        }

        private static Auction GivenValidAuction()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            auction.MarkPendingEventsAsHandled();
            return auction;
        }
    }
}
