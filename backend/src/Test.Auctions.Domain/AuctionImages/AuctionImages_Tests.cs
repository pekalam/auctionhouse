using Auctions.DomainEvents;
using Auctions.Tests.Base.Domain.ModelBuilders;
using Core.DomainFramework;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Auctions.Domain.Tests
{
    [Trait("Category", "Unit")]
    public class AuctionImages_Tests
    {
        AuctionImages auctionImages = new AuctionImages();


        [Fact]
        public void Added_images_are_placed_on_next_available_place_on_the_list()
        {
            //arrange
            var image = new GivenAuctionImage().Build();

            //act & assert
            for (var currentPlace = 0; currentPlace < 2; currentPlace++)
            {
                auctionImages.AddImage(image);
                auctionImages.Count().Should().Be(currentPlace + 1);
                auctionImages[currentPlace].Should().Be(image);
                AllOtherShouldBeNull(auctionImages, currentPlace);
            }

            static void AllOtherShouldBeNull(AuctionImages auctionImages, int currentPlace)
            {
                for (var i = currentPlace + 1; i < AuctionConstantsFactory.MaxImages; i++)
                {
                    auctionImages[i].Should().BeNull();
                }
            }
        }

        [Fact]
        public void Replaces_image_at_given_image_number()
        {
            auctionImages = GivenAuctionImagesWithAlreadyAddedImage();
            var image2 = new GivenAuctionImage().Build();

            auctionImages[0] = image2;

            auctionImages.Count().Should().Be(1);
            auctionImages[0].Should().Be(image2);
        }

        private AuctionImages GivenAuctionImagesWithAlreadyAddedImage()
        {
            var image1 = new GivenAuctionImage().Build();
            auctionImages.AddImage(image1);
            return auctionImages;
        }

        [Fact]
        public void Inserts_image_at_given_image_number()
        {
            //arrange
            var image1 = new GivenAuctionImage().Build();
            var image2 = new GivenAuctionImage().Build();

            //act
            auctionImages[1] = image1;
            auctionImages[AuctionConstantsFactory.MaxImages - 1] = image2;

            //assert
            auctionImages[0].Should().BeNull();
            AllFromThirdToLastShouldBeNull(auctionImages);
            auctionImages[1].Should().Be(image1);
            auctionImages[AuctionConstantsFactory.MaxImages - 1].Should().Be(image2);

            static void AllFromThirdToLastShouldBeNull(AuctionImages auctionImages)
            {
                for (var i = 2; i < AuctionConstantsFactory.MaxImages - 1; i++)
                {
                    auctionImages[i].Should().BeNull();
                }
            }
        }

        [Fact]
        public void Throws_when_assigning_image_with_invalid_number()
        {
            Assert.Throws<DomainException>(() => auctionImages[AuctionConstantsFactory.MaxImages]);
            Assert.Throws<DomainException>(() => auctionImages[-1]);
            Assert.Throws<DomainException>(() => auctionImages[AuctionConstantsFactory.MaxImages] = new GivenAuctionImage().Build());
            Assert.Throws<DomainException>(() => auctionImages[-1] = new GivenAuctionImage().Build());
        }

        [Fact]
        public void Count_takes_into_account_only_not_null_images()
        {
            auctionImages.Count().Should().Be(0);
        }

        [Fact]
        public void ClearAll_removes_all_images()
        {
            var image1 = new GivenAuctionImage().Build();
            auctionImages[1] = image1;

            auctionImages.ClearAll();

            AllImagesShouldBeNull();

            void AllImagesShouldBeNull()
            {
                for (var i = 0; i < AuctionConstantsFactory.MaxImages; i++)
                {
                    auctionImages[i].Should().BeNull();
                }
            }
        }

        [Fact]
        public void FromSizeIds_recreates_same_AutionImages_object()
        {
            //arrange
            AuctionImage image1, image2, image3;
            GivenAuctionImagesInsertedWith123Number(out image1, out image2, out image3);
            var sizeIds = new
            {
                Size1Ids = auctionImages.Size1Ids.ToArray(),
                Size2Ids = auctionImages.Size2Ids.ToArray(),
                Size3Ids = auctionImages.Size3Ids.ToArray(),
            };

            //act
            var fromIds = AuctionImages.FromSizeIds(sizeIds.Size1Ids, sizeIds.Size2Ids, sizeIds.Size3Ids);

            //assert
            fromIds.Count().Should().Be(3);
            fromIds[1].Should().BeEquivalentTo(image1);
            fromIds[2].Should().BeEquivalentTo(image2);
            fromIds[3].Should().BeEquivalentTo(image3);
        }

        private void GivenAuctionImagesInsertedWith123Number(out AuctionImage image1, out AuctionImage image2, out AuctionImage image3)
        {
            image1 = new GivenAuctionImage().Build();
            image2 = new GivenAuctionImage().Build();
            image3 = new GivenAuctionImage().Build();
            auctionImages[1] = image1;
            auctionImages[2] = image2;
            auctionImages[3] = image3;
        }
    }
}
