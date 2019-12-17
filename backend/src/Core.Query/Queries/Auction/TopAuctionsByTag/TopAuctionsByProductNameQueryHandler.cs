using System.Threading;
using System.Threading.Tasks;
using Core.Common.Domain.Products;
using Core.Common.Query;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using Core.Query.Views;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.TopAuctionsByTag
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
            var topByProductName = await _dbContext.TopAuctionsByProductName.Find(t => t.CanonicalName == canonicalName)
                .Skip(request.Page * TopAuctionsInTagQuery.MAX_PER_PAGE)
                .Limit(TopAuctionsInTagQuery.MAX_PER_PAGE)
                .FirstOrDefaultAsync();

            return topByProductName;
        }
    }
}