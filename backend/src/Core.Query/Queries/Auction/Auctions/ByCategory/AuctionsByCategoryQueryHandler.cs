using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Domain.Categories;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.Auctions.ByCategory
{
    public class AuctionsByCategoryQueryHandler : AuctionsQueryHandlerBase, IRequestHandler<AuctionsByCategoryQuery, IEnumerable<AuctionsQueryResult>>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly CategoryBuilder _categoryBuilder;

        public AuctionsByCategoryQueryHandler(ReadModelDbContext dbContext, CategoryBuilder categoryBuilder)
        {
            _dbContext = dbContext;
            _categoryBuilder = categoryBuilder;
        }

        public async Task<IEnumerable<AuctionsQueryResult>> Handle(AuctionsByCategoryQuery request,
            CancellationToken cancellationToken)
        {
            var mapper = MapperConfigHolder.Configuration.CreateMapper();
            var category = _categoryBuilder.FromCategoryNamesList(request.CategoryNames);
            var catFilter = Builders<AuctionReadModel>.Filter.Eq(f => f.Category, category);

            var filtersArr = new List<FilterDefinition<AuctionReadModel>>()
            {
                catFilter
            };
            var filtersFromBase = CreateFilterDefs(request);
            filtersArr.AddRange(filtersFromBase);

            var auctions = await _dbContext.AuctionsReadModel
                .Find(Builders<AuctionReadModel>.Filter.And(filtersArr))
                .Skip(request.Page * PageSize)
                .Project(model => mapper.Map<AuctionsQueryResult>(model))
                .Limit(PageSize)
                .ToListAsync();
            return auctions;
        }
    }
}