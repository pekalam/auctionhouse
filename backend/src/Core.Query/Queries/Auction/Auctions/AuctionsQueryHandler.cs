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
                filtersArr.Add(Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowPrice, null));
            }
            if (request.AuctionTypeQuery == AuctionTypeQuery.BuyNow)
            {
                filtersArr.Add(Builders<AuctionReadModel>.Filter.Ne(f => f.BuyNowPrice, null));
            }

            if (request.ConditionQuery != ConditionQuery.All)
            {
                filtersArr.Add(Builders<AuctionReadModel>.Filter.Eq(f => f.Product.Condition, (Condition)request.ConditionQuery));
            }

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