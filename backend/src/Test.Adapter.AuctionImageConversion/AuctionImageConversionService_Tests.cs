using Adapter.AuctionImageConversion;
using Auctions.Domain;
using Auctions.Domain.Services;
using FluentAssertions;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Test.Adapter.AuctionImageConversion
{
    public class AuctionImageConversionService_Tests
    {
        private IAuctionImageConversion service;

        public AuctionImageConversionService_Tests()
        {
            service = new AuctionImageConversionService();
        }

        public void SetUp()
        {
        }

        [Theory]
        [InlineData("./auctionImageConversionService_data/1200x600.jpg", true)]
        [InlineData("./auctionImageConversionService_data/500x900.jpg", true)]
        [InlineData("./auctionImageConversionService_data/1200x600.png", true)]
        [InlineData("./auctionImageConversionService_data/500x900.png", true)]
        [InlineData("./auctionImageConversionService_data/500x900.gif", false)]
        [InlineData("./auctionImageConversionService_data/500x900.bmp", false)]
        public void ValidateImage_returns_valid_result(string path, bool shouldBeValid)
        {
            var ext = path.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
            var imgRepresentation =
                new AuctionImageRepresentation(new AuctionImageMetadata(ext), File.ReadAllBytes(path));

            var isValid = service.ValidateImage(imgRepresentation, new[] { "jpg", "png" });

            Assert.Equal(shouldBeValid, isValid);
        }

        [Fact]
        public void ValidateImage_when_invalid_img_representation_returns_false()
        {
            var imgRepresentation = new AuctionImageRepresentation(new AuctionImageMetadata("jpg"), Enumerable.Range(0, 20).Select(i => (byte)i).ToArray());

            var isValid = service.ValidateImage(imgRepresentation, new[] { "jpg", "png" });

            Assert.False(isValid);
        }

        [InlineData("./auctionImageConversionService_data/1200x600.jpg", 300, 300)]
        public void ConvertTo_returns_image_with_valid_size(string path, int desiredW, int desiredH)
        {
            var ext = path.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
            var imgRepresentation =
                new AuctionImageRepresentation(new AuctionImageMetadata(ext), File.ReadAllBytes(path));
            var size = new AuctionImageSize(desiredW, desiredH);

            var converted = service.ConvertTo(imgRepresentation, size);

            converted.Should().NotBeNull();
            converted.Img.Length.Should().BeGreaterThan(0);
            converted.Metadata.Should().NotBeNull();

            using (var img = Image.Load(converted.Img))
            {
                img.Width.Should().BeLessOrEqualTo(desiredW);
                img.Height.Should().BeLessOrEqualTo(desiredH);
                img.Width.Should().BeGreaterThan(0);
                img.Height.Should().BeGreaterThan(0);
            }
        }
    }
}
