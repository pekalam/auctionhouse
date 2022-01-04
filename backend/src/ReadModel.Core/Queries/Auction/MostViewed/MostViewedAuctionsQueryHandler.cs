using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.MostViewed
{
    public class MostViewedAuctionsQueryHandler : QueryHandlerBase<MostViewedAuctionsQuery, IEnumerable<MostViewedAuctionsResult>>
    {
        private readonly ReadModelDbContext _dbContext;

        public MostViewedAuctionsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<IEnumerable<MostViewedAuctionsResult>> HandleQuery(MostViewedAuctionsQuery request, CancellationToken cancellationToken)
        {
            var mapper = MapperConfigHolder.Configuration.CreateMapper();

            var result = await _dbContext.AuctionsReadModel
                .Find(model => !model.Archived && model.Views > MostViewedAuctionsQuery.VIEWS_MIN)
                .Project(model => mapper.Map<MostViewedAuctionsResult>(model))
                .Limit(MostViewedAuctionsQuery.AUCTIONS_LIMIT)
                .ToListAsync();

            return result;
        }
    }
}