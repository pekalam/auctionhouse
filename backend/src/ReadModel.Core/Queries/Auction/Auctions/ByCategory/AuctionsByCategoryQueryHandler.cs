using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Domain.Categories;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.Auctions.ByCategory
{
    public class AuctionsByCategoryQueryHandler : AuctionsQueryHandlerBase<AuctionsByCategoryQuery>
    {
        private readonly ReadModelDbContext _dbContext;

        public AuctionsByCategoryQueryHandler(ReadModelDbContext dbContext, CategoryBuilder categoryBuilder)
        {
            _dbContext = dbContext;
        }

        protected async override Task<AuctionsQueryResult> HandleQuery(AuctionsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var mapper = MapperConfigHolder.Configuration.CreateMapper();

            var filtersArr = new List<FilterDefinition<AuctionRead>>()
            {
                Builders<AuctionRead>.Filter.Eq(f => f.Category.Name, request.CategoryNames[0]),
                Builders<AuctionRead>.Filter.Eq(f => f.Category.SubCategory.Name, request.CategoryNames[1]),
                Builders<AuctionRead>.Filter.Eq(f => f.Category.SubCategory.SubCategory.Name, request.CategoryNames[2]),
            };
            var filtersFromBase = CreateFilterDefs(request);
            filtersArr.AddRange(filtersFromBase);

            var auctions = await _dbContext.AuctionsReadModel
                .Find(Builders<AuctionRead>.Filter.And(filtersArr))
                .Skip(request.Page * PageSize)
                .Project(model => mapper.Map<AuctionListItem>(model))
                .Limit(PageSize)
                .Sort(GetDefaultSorting())
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