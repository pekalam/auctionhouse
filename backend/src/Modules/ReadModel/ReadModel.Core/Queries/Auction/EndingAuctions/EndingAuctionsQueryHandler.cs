using MongoDB.Driver;
using ReadModel.Contracts.Queries.Auction.EndingAuctions;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.EndingAuctions
{
    public class EndingAuctionsQueryHandler : QueryHandlerBase<EndingAuctionsQuery, IEnumerable<Contracts.Views.EndingAuctions>>
    {
        private readonly ReadModelDbContext _dbContext;

        public EndingAuctionsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        protected override async Task<IEnumerable<Contracts.Views.EndingAuctions>> HandleQuery(EndingAuctionsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _dbContext.EndingAuctionsCollection.Find(FilterDefinition<Contracts.Views.EndingAuctions>.Empty)
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}