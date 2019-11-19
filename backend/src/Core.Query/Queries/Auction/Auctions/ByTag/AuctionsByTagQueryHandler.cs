using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.Auctions.ByTag
{
    public class AuctionsByTagQueryHandler : AuctionsQueryHandlerBase<AuctionsByTagQuery>
    {
        private readonly ReadModelDbContext _dbContext;

        public AuctionsByTagQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<IEnumerable<AuctionsQueryResult>> HandleQuery(AuctionsByTagQuery request, CancellationToken cancellationToken)
        {
            var mapper = MapperConfigHolder.Configuration.CreateMapper();
            var tagFilter = Builders<AuctionRead>.Filter
                .AnyIn(model => model.Tags, new[] { request.Tag });

            var filtersArr = CreateFilterDefs(request);
            filtersArr.Add(tagFilter);

            var auctions = await _dbContext.AuctionsReadModel
                .Find(Builders<AuctionRead>.Filter.And(filtersArr))
                .Skip(request.Page * PageSize)
                .Project(model => mapper.Map<AuctionsQueryResult>(model))
                .Limit(PageSize)
                .ToListAsync();
            return auctions;
        }
    }
}