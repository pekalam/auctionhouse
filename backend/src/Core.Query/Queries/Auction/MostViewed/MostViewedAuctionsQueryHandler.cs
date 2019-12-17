using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Query;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.MostViewed
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
                .Find(model => model.Views > MostViewedAuctionsQuery.VIEWS_MIN)
                .Project(model => mapper.Map<MostViewedAuctionsResult>(model))
                .Limit(MostViewedAuctionsQuery.AUCTIONS_LIMIT)
                .ToListAsync();

            return result;
        }
    }
}