using System.IO;
using System.Linq;
using System.Threading;
using Core.Common.Domain.Auctions;
using FluentAssertions;
using Infrastructure.Repositories.AuctionImage;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Moq;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class AuctionImageRepository_Tests
    {
        private ImageDbContext dbContext;
        private IAuctionImageRepository auctionImageRepository;

        [SetUp]
        public void SetUp()
        {
            var settings = new ImageDbSettings()
            {
                
                ConnectionString = TestContextUtils.GetParameterOrDefault("mongodb-connection-string", "mongodb://localhost:27017"),
                DatabaseName = "appDb"
            };
            var dbContext = new ImageDbContext(settings);
            this.dbContext = dbContext;
            auctionImageRepository = new AuctionImageRepository(dbContext, Mock.Of<ILogger<AuctionImageRepository>>());
        }

        [TearDown]
        public void TearDown()
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

        [Test]
        public void AddImage_when_valid_file_adds_image()
        {
            var testFile = File.ReadAllBytes("./test_image.jpg");
            var imgRepresentation = new AuctionImageRepresentation(new AuctionImageMetadata("jpg"), testFile);
            auctionImageRepository.Add("img1", imgRepresentation);
            var fetched = auctionImageRepository.Find("img1");

            fetched.Metadata.IsAssignedToAuction.Should()
                .BeFalse();
            fetched.Should()
                .NotBeNull();
            fetched.Img.Length.Should()
                .Be(testFile.Length);
        }

        [Test]
        public void UpdateMetadata_when_file_exists_changes_metadata()
        {
            var testFile = File.ReadAllBytes("./test_image.jpg");
            var imgRepresentation = new AuctionImageRepresentation(new AuctionImageMetadata("jpg"), testFile);
            auctionImageRepository.Add("img1", imgRepresentation);

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
            var testFile = File.ReadAllBytes("./test_image.jpg");
            var imgRepresentation = new AuctionImageRepresentation(new AuctionImageMetadata("jpg"), testFile);
            auctionImageRepository.Add(imageId, imgRepresentation);
        }

        [Test]
        public void UpdateManyMetadata_when_files_exists_changes_metadata()
        {
            AddTestImageToRepository("img1");
            AddTestImageToRepository("img2");

            auctionImageRepository.UpdateManyMetadata(new []{"img1", "img2"}, new AuctionImageMetadata("jpg")
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
    }
}