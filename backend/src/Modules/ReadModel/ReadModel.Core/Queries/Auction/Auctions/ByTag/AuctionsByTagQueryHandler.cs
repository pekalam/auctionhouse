using AutoMapper;
using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Contracts.Queries.Auction.Auctions;
using ReadModel.Contracts.Queries.Auction.Auctions.ByTag;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.Auctions.ByTag
{
    public class AuctionsByTagQueryHandler : AuctionsQueryHandlerBase<AuctionsByTagQuery>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IMapper _mapper;

        public AuctionsByTagQueryHandler(ReadModelDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        protected async override Task<AuctionsQueryResult> HandleQuery(AuctionsByTagQuery request, CancellationToken cancellationToken)
        {
            var tagFilter = Builders<AuctionRead>.Filter
                .AnyIn(model => model.Tags, new[] { request.Tag });

            var queryFilters = CreateFilterDefs(request);
            queryFilters.Add(tagFilter);
            queryFilters.Insert(0, Builders<AuctionRead>.Filter.AuctionIsNotLocked());

            var auctions = await _dbContext.AuctionsReadModel
                .Find(Builders<AuctionRead>.Filter.And(queryFilters))
                .Skip(request.Page * PageSize)
                .Project(model => _mapper.Map<AuctionListItem>(model))
                .Limit(PageSize)
                .Sort(GetDefaultSorting())
                .ToListAsync();

            long total = await _dbContext.AuctionsReadModel.CountDocumentsAsync(Builders<AuctionRead>.Filter.And(queryFilters));

            return new AuctionsQueryResult()
            {
                Auctions = auctions,
                Total = total
            };
        }
    }
}