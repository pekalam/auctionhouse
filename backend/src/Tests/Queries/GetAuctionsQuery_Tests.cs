using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Query;
using Core.Query.Queries.Auction.Auctions;
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
        private AuctionsQueryHandler queryHandler;

        [SetUp]
        public void SetUp()
        {
            _categoryTreeService = TestDepedencies.Instance.Value.CategoryTreeService;
            _dbContext = TestDepedencies.Instance.Value.DbContext;
            queryHandler = new AuctionsQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
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
            var testCategory = new Category(testCategoryTree.SubCategories[0]
                .CategoryName, 0);
            testCategory.SubCategory = new Category(testCategoryTree.SubCategories[0]
                .SubCategories[0]
                .CategoryName, 1);
            testCategory.SubCategory.SubCategory = new Category(testCategoryTree.SubCategories[0]
                .SubCategories[0]
                .SubCategories[0]
                .CategoryName, 2);

            var auctions = new AuctionReadModel[AuctionsQueryHandler.PageSize * 2];

            for (var i = 0; i < auctions.Length; i++)
            {
                auctions[i] = new AuctionReadModel()
                {
                    ActualPrice = 20,
                    AuctionId = Guid.NewGuid()
                        .ToString(),
                    BuyNowPrice = 21,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(1),
                    Category = testCategory,
                    BuyNowOnly = false,
                    Product = new Product("prod 1", "desc", Condition.Used)
                };
            }

            return auctions;
        }

        private void CompareResults(List<AuctionsQueryResult> results, AuctionReadModel[] stubAuctions)
        {
            var mapper = MapperConfigHolder.Configuration.CreateMapper();
            for (int i = 0; i < results.Count; i++)
            {
                var queryResultFromStub = mapper.Map<AuctionsQueryResult>(stubAuctions.First(model =>
                    model.Id == results[i]
                        .Id));
                results[i]
                    .Should()
                    .BeEquivalentTo(queryResultFromStub, config: options =>
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

            var query = new AuctionsQuery()
            {
                Page = page, CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                }
            };
            //act
            var results = queryHandler.Handle(query, CancellationToken.None)
                .Result.ToList();

            //assert

            if (emptyResultset)
            {
                results.Count.Should()
                    .Be(0);
                return;
            }

            results.Count.Should()
                .Be(AuctionsQueryHandler.PageSize);

            CompareResults(results, stubAuctions);

            stubAuctions.Select(f => f.AuctionId)
                .Except(results.Select(a => a.AuctionId))
                .Count()
                .Should()
                .Be(AuctionsQueryHandler.PageSize);
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
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions.AsSpan(0, AuctionsQueryHandler.PageSize)
                .ToArray());

            var queryHandler = new AuctionsQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query1 = new AuctionsQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                AuctionTypeQuery = AuctionTypeQuery.BuyNowOnly,
            };


            var query2 = new AuctionsQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                AuctionTypeQuery = AuctionTypeQuery.Auction,
            };

            var query3 = new AuctionsQuery()
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
                .Result.ToList();
            var results2 = queryHandler.Handle(query2, CancellationToken.None)
                .Result.ToList();
            var results3 = queryHandler.Handle(query3, CancellationToken.None)
                .Result.ToList();

            //assert
            results1.Count.Should()
                .Be(1);
            results2.Count.Should()
                .Be(1);
            results3.Count.Should()
                .Be(AuctionsQueryHandler.PageSize - 2);
        }


        [Test]
        public void Handle_when_given_valid_condition_filter_params_returns_valid_auctions_page()
        {
            //arrange
            var stubAuctions = GetFakeAuctionsReadModels();
            stubAuctions[0]
                .Product.Condition = Condition.New;
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions.AsSpan(0, AuctionsQueryHandler.PageSize)
                .ToArray());

            var queryHandler = new AuctionsQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query1 = new AuctionsQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                ConditionQuery = ConditionQuery.New
            };


            var query2 = new AuctionsQuery()
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
                .Result.ToList();
            var results2 = queryHandler.Handle(query2, CancellationToken.None)
                .Result.ToList();

            //assert
            results1.Count.Should()
                .Be(1);
            results2.Count.Should()
                .Be(AuctionsQueryHandler.PageSize - 1);
        }


        [Test]
        public void Handle_when_given_valid_buyNowPrice_filter_params_returns_valid_auctions_page()
        {
            //arrange
            var stubAuctions = GetFakeAuctionsReadModels();
            stubAuctions[0]
                .BuyNowPrice = 99;
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions.AsSpan(0, AuctionsQueryHandler.PageSize)
                .ToArray());

            var queryHandler = new AuctionsQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query1 = new AuctionsQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                }, MinBuyNowPrice = 0, MaxBuyNowPrice = 40
            };


            var query2 = new AuctionsQuery()
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
                .Result.ToList();
            var results2 = queryHandler.Handle(query2, CancellationToken.None)
                .Result.ToList();

            //assert
            results1.Count.Should()
                .Be(AuctionsQueryHandler.PageSize - 1);
            results2.Count.Should()
                .Be(1);
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
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions.AsSpan(0, AuctionsQueryHandler.PageSize)
                .ToArray());

            var queryHandler = new AuctionsQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query1 = new AuctionsQuery()
            {
                Page = 0,
                CategoryNames = new List<string>()
                {
                    "Fake category", "Fake subcategory", "Fake subsubcategory 0"
                },
                MinAuctionPrice = 0,
                MaxAuctionPrice = 40
            };


            var query2 = new AuctionsQuery()
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
                .Result.ToList();
            var results2 = queryHandler.Handle(query2, CancellationToken.None)
                .Result.ToList();

            //assert
            results1.Count.Should()
                .Be(AuctionsQueryHandler.PageSize - 1);
            results2.Count.Should()
                .Be(1);
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
            _dbContext.AuctionsReadModel.InsertMany(stubAuctions.AsSpan(0, AuctionsQueryHandler.PageSize)
                .ToArray());

            var queryHandler = new AuctionsQueryHandler(_dbContext, new CategoryBuilder(_categoryTreeService));
            var query1 = new AuctionsQuery()
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


            var query2 = new AuctionsQuery()
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
                .Result.ToList();
            var results2 = queryHandler.Handle(query2, CancellationToken.None)
                .Result.ToList();

            //assert
            results1.Count.Should()
                .Be(1);
            results2.Count.Should()
                .Be(AuctionsQueryHandler.PageSize);
            results2[0].AuctionId
                .Should()
                .Be(stubAuctions[0].AuctionId);
        }
    }
}