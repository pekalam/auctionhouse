using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.Auctions.ByTag
{
    public class AuctionsByTagQuery : AuctionsQueryBase, IRequest<IEnumerable<AuctionsQueryResult>>
    {
        public int Page { get; set; }
        public string Tag { get; set; }
    }

    public class AuctionsByTagQueryHandler : AuctionsQueryHandlerBase,
        IRequestHandler<AuctionsByTagQuery, IEnumerable<AuctionsQueryResult>>
    {
        private readonly ReadModelDbContext _dbContext;

        public AuctionsByTagQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<AuctionsQueryResult>> Handle(AuctionsByTagQuery request, CancellationToken cancellationToken)
        {
            var mapper = MapperConfigHolder.Configuration.CreateMapper();
            var tagFilter = Builders<AuctionRead>.Filter
                .AnyIn(model => model.Tags, new[] {request.Tag});
            
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
