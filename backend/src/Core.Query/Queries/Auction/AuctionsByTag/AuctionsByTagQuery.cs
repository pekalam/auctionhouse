using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.AuctionsByTag
{
    public class AuctionsByTagQuery : IRequest<AuctionsByTagQueryResult>
    {
        public const int MAX_PER_PAGE = 20;

        public string Tag { get; set; }
        public int Page { get; set; }
    }

    public class AuctionsByTagQueryResult
    {
        public IEnumerable<TagsAuctionsReadModel> TagsAuctions { get; set; }
    }


    public class AuctionsByTagQueryHandler : IRequestHandler<AuctionsByTagQuery, AuctionsByTagQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;

        public AuctionsByTagQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public Task<AuctionsByTagQueryResult> Handle(AuctionsByTagQuery request, CancellationToken cancellationToken)
        {
            var tagsAuctions = _dbContext.TagsAuctionsCollection.Find(t => t.Tag == request.Tag)
                .Skip(request.Page * AuctionsByTagQuery.MAX_PER_PAGE)
                .Limit(AuctionsByTagQuery.MAX_PER_PAGE)
                .ToEnumerable();

            return Task.FromResult(new AuctionsByTagQueryResult()
            {
                TagsAuctions = tagsAuctions
            });
        }
    }
}