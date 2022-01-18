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
        public void Adds_image_with_subsequent_numbers()
        {
            var auctionImages = new AuctionImages();
            var image = new GivenAuctionImage().Valid();

            for (int j = 0; j < 2; j++)
            {
                auctionImages.AddImage(image);
                auctionImages.Count().Should().Be(j+1);
                auctionImages[j].Should().Be(image);
                for (int i = j+1; i < AuctionConstantsFactory.MaxImages; i++)
                {
                    auctionImages[i].Should().BeNull();
                }
            }
        }

        [Fact]
        public void Replaces_image_at_given_image_number()
        {
            var auctionImages = new AuctionImages();
            var image1 = new GivenAuctionImage().Valid();

            auctionImages.AddImage(image1);
            var image2 = new GivenAuctionImage().Valid();
            auctionImages[0] = image2;

            auctionImages.Count().Should().Be(1);
            auctionImages[0].Should().Be(image2);
        }

        [Fact]
        public void Inserts_image_at_given_image_number()
        {
            var auctionImages = new AuctionImages();
            var image1 = new GivenAuctionImage().Valid();
            var image2 = new GivenAuctionImage().Valid();

            auctionImages[1] = image1;
            auctionImages[AuctionConstantsFactory.MaxImages-1] = image2;
            for (int i = 0; i < 1; i++)
            {
                auctionImages[i].Should().BeNull();
            }
            for (int i = 2; i < AuctionConstantsFactory.MaxImages - 1; i++)
            {
                auctionImages[i].Should().BeNull();
            }
            auctionImages[1].Should().Be(image1);
            auctionImages[AuctionConstantsFactory.MaxImages - 1].Should().Be(image2);
        }

        [Fact]
        public void Throws_when_assigning_image_with_invalid_number()
        {
            var auctionImages = new AuctionImages();
            Assert.Throws<DomainException>(() => auctionImages[AuctionConstantsFactory.MaxImages]);
            Assert.Throws<DomainException>(() => auctionImages[-1]);
            Assert.Throws<DomainException>(() => auctionImages[AuctionConstantsFactory.MaxImages] = new GivenAuctionImage().Valid());
            Assert.Throws<DomainException>(() => auctionImages[-1] = new GivenAuctionImage().Valid());
        }

        [Fact]
        public void Count_counts_non_null_images()
        {
            var auctionImages = new AuctionImages();

            auctionImages.Count().Should().Be(0);
        }

        [Fact]
        public void Clear_all_should_remove_all_images()
        {
            var auctionImages = new AuctionImages();
            var image1 = new GivenAuctionImage().Valid();

            auctionImages[1] = image1;

            auctionImages.ClearAll();
            for (int i = 0; i < AuctionConstantsFactory.MaxImages; i++)
            {
                auctionImages[i].Should().BeNull();
            }
        }


        [Fact]
        public void FromSizeIds_should_recreate_object()
        {
            var auctionImages = new AuctionImages();
            var image1 = new GivenAuctionImage().Valid();
            var image2 = new GivenAuctionImage().Valid();
            var image3 = new GivenAuctionImage().Valid();

            auctionImages[1] = image1;
            auctionImages[2] = image2;
            auctionImages[3] = image3;

            var ids = new
            {
                Size1Ids = auctionImages.Size1Ids.ToArray(),
                Size2Ids = auctionImages.Size2Ids.ToArray(),
                Size3Ids = auctionImages.Size3Ids.ToArray(),
            };

            var fromIds = AuctionImages.FromSizeIds(ids.Size1Ids, ids.Size2Ids, ids.Size3Ids);
            fromIds.Count().Should().Be(3);
            fromIds[1].Should().BeEquivalentTo(image1);
            fromIds[2].Should().BeEquivalentTo(image2);
            fromIds[3].Should().BeEquivalentTo(image3);
        }


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
