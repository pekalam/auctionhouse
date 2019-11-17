using System;
using System.Threading;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Query.Queries.Auction.SingleAuction;
using Core.Query.ReadModel;
using FluentAssertions;
using FunctionalTests.Utils;
using Infrastructure.Services;
using MongoDB.Driver;
using NUnit.Framework;

namespace FunctionalTests.Queries
{
    [TestFixture]
    public class GetSingleAuctionQuery_test
    {
        private ReadModelDbContext _dbContext;
        private CategoryTreeService _categoryTreeService;
        private AuctionRead stubAuction;

        [SetUp]
        public void SetUp()
        {
            _categoryTreeService = TestDepedencies.Instance.Value.CategoryTreeService;
            _dbContext = TestDepedencies.Instance.Value.DbContext;
            var testCategoryTree = _categoryTreeService.GetCategoriesTree();
            var testCategory = new Category(testCategoryTree.SubCategories[0].CategoryName, 0);
            testCategory.SubCategory = new Category(testCategoryTree.SubCategories[0].SubCategories[0].CategoryName, 1);
            testCategory.SubCategory.SubCategory = new Category(testCategoryTree.SubCategories[0].SubCategories[0].SubCategories[0].CategoryName, 2);
            stubAuction = new AuctionRead()
            {
                ActualPrice = 20,
                AuctionId = Guid.NewGuid().ToString(),
                BuyNowPrice = 21,
                StartDate = DateTime.UtcNow.AddMinutes(12),
                EndDate = DateTime.UtcNow.AddDays(1),
                Category = testCategory,
                Product = new Product("name", "desc", Condition.New)
            };
        }

        [TearDown]
        public void TearDown()
        {
            var filter1 = Builders<AuctionRead>.Filter.Empty;
            var filter2 = Builders<UserRead>.Filter.Empty;
            _dbContext.AuctionsReadModel.DeleteMany(filter1);
            _dbContext.UsersReadModel.DeleteMany(filter2);
        }

        [Test]
        public void Handle_when_called_twice_increments_views()
        {
            //arrange
            _dbContext.AuctionsReadModel.InsertOne(stubAuction);
            var queryHandler = new AuctionQueryHandler(_dbContext);
            var query = new AuctionQuery(stubAuction.AuctionId);

            var result1 = queryHandler.Handle(query, CancellationToken.None).Result;
            var result2 = queryHandler.Handle(query, CancellationToken.None).Result;

            result1.Views.Should()
                .Be(0);
            result2.Views.Should()
                .Be(1);
        }

        [Test]
        public void Handle_when_given_valid_page_returns_auction()
        {
            //arrange
            _dbContext.AuctionsReadModel.InsertOne(stubAuction);
            var queryHandler = new AuctionQueryHandler(_dbContext);
            var query = new AuctionQuery(stubAuction.AuctionId);

            //act
            var result = queryHandler.Handle(query, CancellationToken.None).Result;

            //assert
            result.Should().BeEquivalentTo(stubAuction, config: options =>
            {
                options.Excluding(info => info.StartDate);
                options.Excluding(info => info.EndDate);

                return options;
            });
        }
    }
}
