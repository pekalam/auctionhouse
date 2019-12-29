using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Query;
using Core.Query.Queries.Auction.Auctions;
using Core.Query.Queries.Auction.Auctions.ByCategory;
using Core.Query.ReadModel;
using FluentAssertions;
using FunctionalTests.EventHandling;
using FunctionalTests.Utils;
using Infrastructure.Services;
using MongoDB.Driver;
using NUnit.Framework;

namespace FunctionalTests.Queries
{
    [TestFixture]
    public class GetAuctionsQuery_Tests
    {
        private ReadModelDbContext _dbContext;
        private CategoryTreeService _categoryTreeService;
        private AuctionsByCategoryQueryHandler _byCategoryQueryHandler;

        [SetUp]
        public void SetUp()
        {
            _categoryTreeService = TestDepedencies.Instance.Value.CategoryTreeService;
            _dbContext = TestDepedencies.Instance.Value.DbContext;
            _byCategoryQueryHandler = new AuctionsByCategoryQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
        }

        [TearDown]
        public void TearDown()
        {
            var filter1 = Builders<AuctionRead>.Filter.Empty;
            var filter2 = Builders<UserRead>.Filter.Empty;
            _dbContext.AuctionsReadModel.DeleteMany(filter1);
            _dbContext.UsersReadModel.DeleteMany(filter2);
        }

        private AuctionRead[] GetFakeAuctionsReadModels()
        {
            var testCategoryTree = _categoryTreeService.GetCategoriesTree();
            var testCategory = new Category(testCategoryTree.SubCategories[0]
                .CategoryName, 0);
            testCategory.SubCategory = new Category(testCategoryTree.SubCategories[0]
                .SubCategories[0]
                .CategoryName, 1);
            testCategory.SubCategory.SubCategory = new Category(testCategoryTree.SubCategories[0]
                .SubCategories[0]
                .SubCategories[0]
                .CategoryName, 2);

            var auctions = new AuctionRead[AuctionsByCategoryQueryHandler.PageSize * 2];

            for (var i = 0; i < auctions.Length; i++)
            {
                auctions[i] = new AuctionRead()
                {
                    ActualPrice = 20,
                    AuctionId = Guid.NewGuid()
                        .ToString(),
                    BuyNowPrice = 21,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(1),
                    Category = testCategory,
                    BuyNowOnly = false,
                    Product = new Product("test product name", "example description", Condition.Used)
                };
            }

            return auctions;
        }

        private void CompareResults(AuctionsQueryResult results, AuctionRead[] stubAuctions)
        {
            var mapper = MapperConfigHolder.Configuration.CreateMapper();
            for (int i = 0; i < results.Auctions.Count(); i++)
            {
                var queryResultFromStub = mapper.Map<AuctionsQueryResult>(stubAuctions.First(model =>
                    model.Id == results.Auctions.ElementAt(i)
                        .Id));
                results.Auctions.ElementAt(i)
                    .Should()
                    .BeEquivalentTo(queryResultFromStub.Auctions.ElementAt(i), config: options =>
                    {
                        options.Excluding(info => info.StartDate);
                        options.Excluding(info => info.EndDate);

                        return options;
                    });
            }
        }

        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(2, true)]
        public void Handle_when_given_valid_page_returns_valid_auctions_page(int page, bool emptyResultset)
        {
            //arrange
            var stubAuctions = GetFakeAuctionsReadModels();
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions);

            var query = new AuctionsByCategoryQuery()
            {
                Page = page, CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                }
            };
            //act
            var results = _byCategoryQueryHandler.Handle(query, CancellationToken.None)
                .Result;

            //assert

            if (emptyResultset)
            {
                results.Auctions.Count().Should()
                    .Be(0);
                return;
            }

            results.Auctions.Count().Should()
                .Be(AuctionsByCategoryQueryHandler.PageSize);

            CompareResults(results, stubAuctions);

            stubAuctions.Select(f => f.AuctionId)
                .Except(results.Auctions.Select(a => a.AuctionId))
                .Count()
                .Should()
                .Be(AuctionsByCategoryQueryHandler.PageSize);
        }


        [Test]
        public void Handle_when_given_valid_type_filter_params_returns_valid_auctions_page()
        {
            //arrange
            var stubAuctions = GetFakeAuctionsReadModels();
            stubAuctions[0]
                .BuyNowOnly = true;
            stubAuctions[1]
                .BuyNowOnly = false;
            stubAuctions[1]
                .BuyNowPrice = 0;
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions.AsSpan(0, AuctionsByCategoryQueryHandler.PageSize)
                .ToArray());

            var queryHandler = new AuctionsByCategoryQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query1 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                AuctionTypeQuery = AuctionTypeQuery.BuyNowOnly,
            };


            var query2 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                AuctionTypeQuery = AuctionTypeQuery.Auction,
            };

            var query3 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                AuctionTypeQuery = AuctionTypeQuery.AuctionAndBuyNow,
            };

            //act
            var results1 = queryHandler.Handle(query1, CancellationToken.None)
                .Result;
            var results2 = queryHandler.Handle(query2, CancellationToken.None)
                .Result;
            var results3 = queryHandler.Handle(query3, CancellationToken.None)
                .Result;

            //assert
            results1.Auctions.Count().Should()
                .Be(1);
            results1.Total.Should().Be(1);
            results2.Auctions.Count().Should()
                .Be(1);
            results2.Total.Should().Be(1);
            results3.Auctions.Count().Should()
                .Be(AuctionsByCategoryQueryHandler.PageSize - 2);

            results3.Total.Should()
                .Be(AuctionsByCategoryQueryHandler.PageSize - 2);

        }


        [Test]
        public void Handle_when_given_valid_condition_filter_params_returns_valid_auctions_page()
        {
            //arrange
            var stubAuctions = GetFakeAuctionsReadModels();
            stubAuctions[0]
                .Product.Condition = Condition.New;
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions.AsSpan(0, AuctionsByCategoryQueryHandler.PageSize)
                .ToArray());

            var queryHandler = new AuctionsByCategoryQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query1 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                ConditionQuery = ConditionQuery.New
            };


            var query2 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                ConditionQuery = ConditionQuery.Used
            };

            //act
            var results1 = queryHandler.Handle(query1, CancellationToken.None)
                .Result;
            var results2 = queryHandler.Handle(query2, CancellationToken.None)
                .Result;

            //assert
            results1.Auctions.Count().Should()
                .Be(1);
            results1.Total.Should().Be(1);
            results2.Auctions.Count().Should()
                .Be(AuctionsByCategoryQueryHandler.PageSize - 1);

            results2.Total.Should()
                .Be(AuctionsByCategoryQueryHandler.PageSize - 1);

        }


        [Test]
        public void Handle_when_given_valid_buyNowPrice_filter_params_returns_valid_auctions_page()
        {
            //arrange
            var stubAuctions = GetFakeAuctionsReadModels();
            stubAuctions[0]
                .BuyNowPrice = 99;
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions.AsSpan(0, AuctionsByCategoryQueryHandler.PageSize)
                .ToArray());

            var queryHandler = new AuctionsByCategoryQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query1 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                }, MinBuyNowPrice = 0, MaxBuyNowPrice = 40
            };


            var query2 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                }
                ,MinBuyNowPrice = 90, MaxBuyNowPrice = 99
            };

            //act
            var results1 = queryHandler.Handle(query1, CancellationToken.None)
                .Result;
            var results2 = queryHandler.Handle(query2, CancellationToken.None)
                .Result;

            //assert
            results1.Auctions.Count().Should()
                .Be(AuctionsByCategoryQueryHandler.PageSize - 1);
            results1.Total.Should()
                .Be(AuctionsByCategoryQueryHandler.PageSize - 1);
            results2.Auctions.Count().Should()
                .Be(1);
            results2.Total.Should().Be(1);
        }

        [Test]
        public void Handle_when_valid_auctionPrice_filter_params_returns_valid_auctions_page()
        {
            //arrange
            var stubAuctions = GetFakeAuctionsReadModels();
            stubAuctions[0]
                .ActualPrice = 99;
            foreach (var auction in stubAuctions)
            {
                auction.BuyNowPrice = 0;
            }
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions.AsSpan(0, AuctionsByCategoryQueryHandler.PageSize)
                .ToArray());

            var queryHandler = new AuctionsByCategoryQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query1 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                MinAuctionPrice = 0,
                MaxAuctionPrice = 40
            };


            var query2 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                }
                ,
                MinAuctionPrice = 90,
                MaxAuctionPrice = 99
            };

            //act
            var results1 = queryHandler.Handle(query1, CancellationToken.None)
                .Result;
            var results2 = queryHandler.Handle(query2, CancellationToken.None)
                .Result;

            //assert
            results1.Auctions.Count().Should()
                .Be(AuctionsByCategoryQueryHandler.PageSize - 1);
            results1.Total.Should().Be(AuctionsByCategoryQueryHandler.PageSize - 1);
            results2.Auctions.Count().Should()
                .Be(1);
            results2.Total.Should().Be(1);
        }



        [Test]
        public void Handle_when_valid_auctionPrice_and_buyNowPrice_filter_params_returns_valid_auctions_page()
        {
            //arrange
            var stubAuctions = GetFakeAuctionsReadModels();
            stubAuctions[0]
                .ActualPrice = 20;
            stubAuctions[0]
                .BuyNowPrice = 123;
            foreach (var auction in stubAuctions.Skip(1))
            {
                auction.BuyNowPrice = 0;
            }
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions.AsSpan(0, AuctionsByCategoryQueryHandler.PageSize)
                .ToArray());

            var queryHandler = new AuctionsByCategoryQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query1 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                MinAuctionPrice = 0,
                MaxAuctionPrice = 40,
                MinBuyNowPrice = 1,
                MaxBuyNowPrice = 140
            };


            var query2 = new AuctionsByCategoryQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                }
                ,
                MinAuctionPrice = 0,
                MaxAuctionPrice = 40,
                MinBuyNowPrice = 0,
                MaxBuyNowPrice = 140
            };

            //act
            var results1 = queryHandler.Handle(query1, CancellationToken.None)
                .Result;
            var results2 = queryHandler.Handle(query2, CancellationToken.None)
                .Result;

            //assert
            results1.Auctions.Count().Should()
                .Be(1);
            results1.Total.Should().Be(1);
            results2.Auctions.Count().Should()
                .Be(AuctionsByCategoryQueryHandler.PageSize);
            results2.Total.Should().Be(AuctionsByCategoryQueryHandler.PageSize);
            results2.Auctions.ElementAt(0).AuctionId
                .Should()
                .Be(stubAuctions[0].AuctionId);
        }
    }
}