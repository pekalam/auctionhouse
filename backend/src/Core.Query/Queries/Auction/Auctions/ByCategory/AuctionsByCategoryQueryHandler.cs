using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Domain.Categories;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.Auctions.ByCategory
{
    public class AuctionsByCategoryQueryHandler : AuctionsQueryHandlerBase<AuctionsByCategoryQuery>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly CategoryBuilder _categoryBuilder;

        public AuctionsByCategoryQueryHandler(ReadModelDbContext dbContext, CategoryBuilder categoryBuilder)
        {
            _dbContext = dbContext;
            _categoryBuilder = categoryBuilder;
        }

        protected async override Task<AuctionsQueryResult> HandleQuery(AuctionsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var mapper = MapperConfigHolder.Configuration.CreateMapper();
            var category = _categoryBuilder.FromCategoryNamesList(request.CategoryNames);
            var catFilter = Builders<AuctionRead>.Filter.Eq(f => f.Category, category);

            var filtersArr = new List<FilterDefinition<AuctionRead>>()
            {
                catFilter
            };
            var filtersFromBase = CreateFilterDefs(request);
            filtersArr.AddRange(filtersFromBase);

            var auctions = await _dbContext.AuctionsReadModel
                .Find(Builders<AuctionRead>.Filter.And(filtersArr))
                .Skip(request.Page * PageSize)
                .Project(model => mapper.Map<AuctionListItem>(model))
                .Limit(PageSize)
                .ToListAsync();

            long total = await _dbContext.AuctionsReadModel.CountDocumentsAsync(Builders<AuctionRead>.Filter.And(filtersArr));

            return new AuctionsQueryResult()
            {
                Auctions = auctions,
                Total = total
            };
        }
    }
}