using System.Threading;
using System.Threading.Tasks;
using Core.Query.ReadModel;
using Core.Query.Views;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.TopAuctionsByTag
{
    public class TopAuctionsByTagQuery : IRequest<TopAuctionsInTag>
    {
        public const int MAX_PER_PAGE = 20;

        public string Tag { get; set; }
        public int Page { get; set; }
    }

    public class TopAuctionsByTagQueryHandler : IRequestHandler<TopAuctionsByTagQuery, TopAuctionsInTag>
    {
        private readonly ReadModelDbContext _dbContext;

        public TopAuctionsByTagQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public Task<TopAuctionsInTag> Handle(TopAuctionsByTagQuery request, CancellationToken cancellationToken)
        {
            var tagsAuctions = _dbContext.TagsAuctionsCollection.Find(t => t.Tag == request.Tag)
                .Skip(request.Page * TopAuctionsByTagQuery.MAX_PER_PAGE)
                .Limit(TopAuctionsByTagQuery.MAX_PER_PAGE)
                .FirstOrDefault();

            return Task.FromResult(tagsAuctions);
        }
    }
}