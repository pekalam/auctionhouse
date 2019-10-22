using System.IO;
using System.Threading;
using Core.Common.Domain.Auctions;
using FluentAssertions;
using Infrastructure.Adapters.Repositories.AuctionImage;
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
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "appDb"
            };
            var dbContext = new ImageDbContext(settings);
            this.dbContext = dbContext;
            auctionImageRepository = new AuctionImageRepository(dbContext, Mock.Of<ILogger<AuctionImageRepository>>());
        }

        [TearDown]
        public void TearDown()
        {
             var filter = Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename,
                "img1");
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
            var imgRepresentation = new AuctionImageRepresentation()
            {
                Img = testFile
            };
            auctionImageRepository.AddImage("img1", imgRepresentation);
            Thread.Sleep(2000);
            var fetched = auctionImageRepository.FindImage("img1");

            fetched.Should().NotBeNull();
            fetched.Img.Length.Should().Be(testFile.Length);
        }
    }
}
