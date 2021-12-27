using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using Core.Common.Domain.Auctions;
using FluentAssertions;
using Infrastructure.Services;
using NUnit.Framework;
using SixLabors.ImageSharp;

namespace Test.IntegrationTests
{
    [TestFixture]
    public class AuctionImageConversionService_Tests
    {
        private IAuctionImageConversion service;

        [SetUp]
        public void SetUp()
        {
            service = new AuctionImageConversionService();
        }

        [TestCase("./auctionImageConversionService_data/1200x600.jpg", true)]
        [TestCase("./auctionImageConversionService_data/500x900.jpg", true)]
        [TestCase("./auctionImageConversionService_data/1200x600.png", true)]
        [TestCase("./auctionImageConversionService_data/500x900.png", true)]
        [TestCase("./auctionImageConversionService_data/500x900.gif", false)]
        [TestCase("./auctionImageConversionService_data/500x900.bmp", false)]
        public void ValidateImage_returns_valid_result(string path, bool shouldBeValid)
        {
            var ext = path.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
            var imgRepresentation =
                new AuctionImageRepresentation(new AuctionImageMetadata(ext), File.ReadAllBytes(path));

            var isValid = service.ValidateImage(imgRepresentation, new[] {"jpg", "png"});

            Assert.AreEqual(shouldBeValid, isValid);
        }

        [Test]
        public void ValidateImage_when_invalid_img_representation_returns_false()
        {
            var imgRepresentation = new AuctionImageRepresentation(new AuctionImageMetadata("jpg"), Enumerable.Range(0, 20).Select(i => (byte)i).ToArray());

            var isValid = service.ValidateImage(imgRepresentation, new[] { "jpg", "png" });

            Assert.AreEqual(false, isValid);
        }

        [TestCase("./auctionImageConversionService_data/1200x600.jpg", 300, 300)]
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
