using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.Auctions
{
    public class AuctionsQueryHandler : IRequestHandler<AuctionsQuery, IEnumerable<AuctionsQueryResult>>
    {
        public readonly static int PageSize = 10;
        private readonly ReadModelDbContext _dbContext;
        private readonly CategoryBuilder _categoryBuilder;

        public AuctionsQueryHandler(ReadModelDbContext dbContext, CategoryBuilder categoryBuilder)
        {
            _dbContext = dbContext;
            _categoryBuilder = categoryBuilder;
        }

        private void CreateBuyNowPriceFilter(List<FilterDefinition<AuctionReadModel>> filtersArr, AuctionsQuery request)
        {
            if (request.MinBuyNowPrice != request.MaxBuyNowPrice
                && request.MinBuyNowPrice < request.MaxBuyNowPrice
                && (request.AuctionTypeQuery == AuctionTypeQuery.All || request.AuctionTypeQuery == AuctionTypeQuery.AuctionAndBuyNow
                    || request.AuctionTypeQuery == AuctionTypeQuery.BuyNowOnly))
            {
                var filter1 = Builders<AuctionReadModel>.Filter.Gte(f => f.BuyNowPrice, request.MinBuyNowPrice);
                var filter2 = Builders<AuctionReadModel>.Filter.Lte(f => f.BuyNowPrice, request.MaxBuyNowPrice);
                var priceFilter = Builders<AuctionReadModel>.Filter.And(filter1, filter2);

                if (request.AuctionTypeQuery == AuctionTypeQuery.BuyNowOnly)
                {
                    //buy now only
                    filtersArr.Add(Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowOnly, true));
                }
                filtersArr.Add(priceFilter);
            }
        }

        private void CreateAuctionPriceFilter(List<FilterDefinition<AuctionReadModel>> filtersArr,
            AuctionsQuery request)
        {
            if (request.MinAuctionPrice != request.MaxAuctionPrice
                && request.MinAuctionPrice < request.MaxAuctionPrice
                && (request.AuctionTypeQuery == AuctionTypeQuery.All || request.AuctionTypeQuery == AuctionTypeQuery.AuctionAndBuyNow
                    || request.AuctionTypeQuery == AuctionTypeQuery.Auction))
            {
                var filter1 = Builders<AuctionReadModel>.Filter.Gte(f => f.ActualPrice, request.MinAuctionPrice);
                var filter2 = Builders<AuctionReadModel>.Filter.Lte(f => f.ActualPrice, request.MaxAuctionPrice);

                var priceFilter = Builders<AuctionReadModel>.Filter.And(filter1, filter2);

                if (request.AuctionTypeQuery == AuctionTypeQuery.Auction)
                {
                    filtersArr.Add(Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowOnly, false));
                }
                filtersArr.Add(priceFilter);
            }
        }

        private List<FilterDefinition<AuctionReadModel>> CreateFilterDefs(AuctionsQuery request)
        {
            var category = _categoryBuilder.FromCategoryNamesList(request.CategoryNames);
            var catFilter = Builders<AuctionReadModel>.Filter.Eq(f => f.Category, category);

            var filtersArr = new List<FilterDefinition<AuctionReadModel>>()
            {
                catFilter
            };
            if (request.AuctionTypeQuery == AuctionTypeQuery.Auction)
            {
                var f1 = Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowOnly, false);
                var f2 = Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowPrice, 0);
                filtersArr.Add(Builders<AuctionReadModel>.Filter.And(f1, f2));
            }

            if (request.AuctionTypeQuery == AuctionTypeQuery.BuyNowOnly)
            {
                filtersArr.Add(Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowOnly, true));
            }

            if (request.AuctionTypeQuery == AuctionTypeQuery.AuctionAndBuyNow)
            {
                var f1 = Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowOnly, false);
                var f2 = Builders<AuctionReadModel>.Filter.Gt(f => f.BuyNowPrice, 0);
                filtersArr.Add(Builders<AuctionReadModel>.Filter.And(f1,f2));
            }

            if (request.ConditionQuery != ConditionQuery.All)
            {
                filtersArr.Add(Builders<AuctionReadModel>.Filter.Eq(f => f.Product.Condition,
                    (Condition) request.ConditionQuery));
            }

            CreateBuyNowPriceFilter(filtersArr, request);

            CreateAuctionPriceFilter(filtersArr, request);

            return filtersArr;
        }

        public async Task<IEnumerable<AuctionsQueryResult>> Handle(AuctionsQuery request,
            CancellationToken cancellationToken)
        {
            var mapper = MapperConfigHolder.Configuration.CreateMapper();
            var filtersArr = CreateFilterDefs(request);

            var auctions = await _dbContext.AuctionsReadModel
                .Find(Builders<AuctionReadModel>.Filter.And(filtersArr))
                .Skip(request.Page * PageSize)
                .Project(model => mapper.Map<AuctionsQueryResult>(model))
                .Limit(PageSize)
                .ToListAsync();
            return auctions;
            ;
        }
    }
}