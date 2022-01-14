using System.Threading;
using System.Threading.Tasks;
using Auctions.Domain;
using MongoDB.Driver;
using ReadModel.Core.Model;
using ReadModel.Core.Views;

namespace ReadModel.Core.Queries.Auction.TopAuctionsByTag
{
    public class TopAuctionsByProductNameQueryHandler : QueryHandlerBase<TopAuctionsByProductNameQuery, TopAuctionsByProductName>
    {
        private readonly ReadModelDbContext _dbContext;

        public TopAuctionsByProductNameQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<TopAuctionsByProductName> HandleQuery(TopAuctionsByProductNameQuery request, CancellationToken cancellationToken)
        {
            var productName = request.ProductName.Trim();
            var canonicalName = Product.CanonicalizeProductName(productName);
            var topByProductName = await _dbContext.TopAuctionsByProductNameCollection.Find(t => t.CanonicalName == canonicalName)
                .Skip(request.Page * TopAuctionsInTagQuery.MAX_PER_PAGE)
                .Limit(TopAuctionsInTagQuery.MAX_PER_PAGE)
                .FirstOrDefaultAsync();

            return topByProductName;
        }
    }
}