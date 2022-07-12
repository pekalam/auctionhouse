using Adapter.MongoDb.AuctionImage;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Moq;
using System;
using System.IO;
using System.Linq;
using TestConfigurationAccessor;
using Xunit;

namespace IntegrationTests
{
    [Trait("Category", "Integration")]
    public class AuctionImageRepository_Tests : IDisposable
    {
        private ImageDbContext dbContext;
        private IAuctionImageRepository auctionImageRepository;

        public AuctionImageRepository_Tests()
        {
            var settings = TestConfig.Instance.GetDbSettings();
            dbContext = new ImageDbContext(settings);
            auctionImageRepository = new AuctionImageRepository(dbContext, Mock.Of<ILogger<AuctionImageRepository>>());
        }

        private static AuctionImageRepresentation GivenImageRepresentation(byte[] testFile)
        {
            return new AuctionImageRepresentation(new AuctionImageMetadata("jpg"), testFile);
        }

        private static byte[] GivenTestImageBytes()
        {
            return File.ReadAllBytes("./test_image.jpg");
        }

        [Fact]
        public void Can_add_image_to_db()
        {
            var testImageBytes = GivenTestImageBytes();
            AddTestImageToRepository("img1");
            var fetched = auctionImageRepository.Find("img1");

            fetched.Should()
                .NotBeNull();
            fetched.Metadata.IsAssignedToAuction.Should()
                .BeFalse();
            fetched.Img.Length.Should()
                .Be(testImageBytes.Length);
        }

        [Fact]
        public void Can_change_existing_image_metadata()
        {
            var testFile = GivenTestImageBytes();
            AddTestImageToRepository("img1");

            auctionImageRepository.UpdateMetadata("img1", new AuctionImageMetadata("jpg")
            {
                IsAssignedToAuction = true
            });
            var fetched = auctionImageRepository.Find("img1");

            fetched.Metadata.IsAssignedToAuction.Should()
                .BeTrue();
            fetched.Should()
                .NotBeNull();
            fetched.Img.Length.Should()
                .Be(testFile.Length);
        }

        private void AddTestImageToRepository(string imageId)
        {
            var testFile = GivenTestImageBytes();
            var imgRepresentation = GivenImageRepresentation(testFile);
            auctionImageRepository.Add(imageId, imgRepresentation);
        }

        [Fact]
        public void Can_image_metadata_of_existing_images()
        {
            AddTestImageToRepository("img1");
            AddTestImageToRepository("img2");

            auctionImageRepository.UpdateManyMetadata(new[] { "img1", "img2" }, new AuctionImageMetadata("jpg")
            {
                IsAssignedToAuction = true
            });
            var fetched1 = auctionImageRepository.Find("img1");
            var fetched2 = auctionImageRepository.Find("img2");


            fetched1.Metadata.IsAssignedToAuction.Should()
                .BeTrue();
            fetched2.Metadata.IsAssignedToAuction.Should()
                .BeTrue();
        }

        public void Dispose()
        {
            var filter = Builders<GridFSFileInfo>.Filter.In(f => f.Filename,
                Enumerable.Range(0, 10).Select(i => $"img{i}"));
            using (var c = dbContext.Bucket.Find(filter))
            {
                foreach (var i in c.ToEnumerable())
                {
                    dbContext.Bucket.Delete(i.Id);
                }
            }
        }
    }
}