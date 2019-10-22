using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Common.Domain.Categories;
using Core.Query;
using Core.Query.Queries.Auction.Auctions;
using Core.Query.ReadModel;
using FluentAssertions;
using FunctionalTests.EventHandling;
using FunctionalTests.Utils;
using Infrastructure.Adapters.Services;
using MongoDB.Driver;
using NUnit.Framework;

namespace FunctionalTests.Queries
{
    [TestFixture]
    public class GetAuctionsQuery_Tests
    {
        private ReadModelDbContext _dbContext;
        private CategoryTreeService _categoryTreeService;

        [SetUp]
        public void SetUp()
        {
            _categoryTreeService = TestDepedencies.Instance.Value.CategoryTreeService;
            _dbContext = TestDepedencies.Instance.Value.DbContext;
        }

        [TearDown]
        public void TearDown()
        {
            var filter1 = Builders<AuctionReadModel>.Filter.Empty;
            var filter2 = Builders<UserReadModel>.Filter.Empty;
            _dbContext.AuctionsReadModel.DeleteMany(filter1);
            _dbContext.UsersReadModel.DeleteMany(filter2);
        }

        private AuctionReadModel[] GetFakeAuctionsReadModels()
        {
            var testCategoryTree = _categoryTreeService.GetCategoriesTree();
            var testCategory = new Category(testCategoryTree.SubCategories[0].CategoryName, 0);
            testCategory.SubCategory = new Category(testCategoryTree.SubCategories[0].SubCategories[0].CategoryName, 1);
            testCategory.SubCategory.SubCategory = new Category(testCategoryTree.SubCategories[0].SubCategories[0].SubCategories[0].CategoryName, 2);

            var auctions = new AuctionReadModel[AuctionsQueryHandler.PageSize * 2];

            for (var i = 0; i < auctions.Length; i++)
            {
                auctions[i] = new AuctionReadModel()
                {
                    ActualPrice = 20,
                    AuctionId = Guid.NewGuid().ToString(),
                    BuyNowPrice = 21,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(1),
                    Category = testCategory
                };
            }

            return auctions;
        }

        [Test]
        public void Handle_when_given_valid_page_returns_auctions()
        {
            //arrange
            var stubAuctions = GetFakeAuctionsReadModels();
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions);
            Thread.Sleep(2000);

            var queryHandler = new AuctionsQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query = new AuctionsQuery() { Page = 1, CategoryNames = new List<string>()
            {
                "Fake category", "Fake subcategory", "Fake subsubcategory 0"
            }
            };
            //act
            var results = queryHandler.Handle(query, CancellationToken.None).Result.ToList();

            //assert
            var mapper = MapperConfigHolder.Configuration.CreateMapper();
            results.Count.Should().Be(AuctionsQueryHandler.PageSize);
            for(int i = 0; i < results.Count; i++)
            {
                var queryResultFromStub = mapper.Map<AuctionsQueryResult>(stubAuctions.First(model => model.Id == results[i].Id));
                results[i].Should().BeEquivalentTo(queryResultFromStub, config: options =>
                    {
                        options.Excluding(info => info.StartDate);
                        options.Excluding(info => info.EndDate);

                        return options;
                    });
                
            }
        }
    }
}
