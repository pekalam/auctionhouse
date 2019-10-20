using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Domain.Categories;
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

        public async Task<IEnumerable<AuctionsQueryResult>> Handle(AuctionsQuery request, CancellationToken cancellationToken)
        {
            var category = _categoryBuilder.FromCategoryNamesList(request.CategoryNames);
            var filter = Builders<AuctionReadModel>.Filter.Eq(f => f.Category, category);
            var mapper = MapperConfigHolder.Configuration.CreateMapper();

            var auctions = await _dbContext.AuctionsReadModel
                .Find(filter).Skip(request.Page * PageSize)
                .Project(model => mapper.Map<AuctionsQueryResult>(model))
                .Limit(PageSize)
                .ToListAsync();
            return auctions;
;        }
    }
}