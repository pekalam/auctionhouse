using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.EndingAuctions
{
    public class EndingAuctionsQueryHandler : QueryHandlerBase<EndingAuctionsQuery, IEnumerable<Views.EndingAuctions>>
    {
        private readonly ReadModelDbContext _dbContext;

        public EndingAuctionsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        protected override async Task<IEnumerable<Views.EndingAuctions>> HandleQuery(EndingAuctionsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _dbContext.EndingAuctionsCollection.Find(FilterDefinition<Views.EndingAuctions>.Empty)
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}