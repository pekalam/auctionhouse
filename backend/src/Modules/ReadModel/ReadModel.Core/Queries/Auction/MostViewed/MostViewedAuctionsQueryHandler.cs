using AutoMapper;
using MongoDB.Driver;
using ReadModel.Contracts.Queries.Auction.MostViewed;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.MostViewed
{
    public class MostViewedAuctionsQueryHandler : QueryHandlerBase<MostViewedAuctionsQuery, IEnumerable<MostViewedAuctionsResult>>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IMapper _mapper;

        public MostViewedAuctionsQueryHandler(ReadModelDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        protected async override Task<IEnumerable<MostViewedAuctionsResult>> HandleQuery(MostViewedAuctionsQuery request, CancellationToken cancellationToken)
        {
            var result = await _dbContext.AuctionsReadModel
                .Find(model => !model.Locked && !model.Archived && model.Views > MostViewedAuctionsQuery.VIEWS_MIN)
                .Project(model => _mapper.Map<MostViewedAuctionsResult>(model))
                .Limit(MostViewedAuctionsQuery.AUCTIONS_LIMIT)
                .ToListAsync();

            return result;
        }
    }
}