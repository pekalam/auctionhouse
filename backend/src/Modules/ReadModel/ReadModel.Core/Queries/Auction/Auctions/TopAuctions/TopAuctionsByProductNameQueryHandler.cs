using System.Threading;
using System.Threading.Tasks;
using Auctions.Domain;
using MongoDB.Driver;
using ReadModel.Core.Model;
using ReadModel.Core.Views;

namespace ReadModel.Core.Queries.Auction.TopAuctionsByTag
{
    public class TopAuctionsByProductNameQueryHandler : QueryHandlerBase<TopAuctionsByProductNameQuery, TopAuctionsByProductName[]>
    {
        private readonly ReadModelDbContext _dbContext;

        public TopAuctionsByProductNameQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<TopAuctionsByProductName[]> HandleQuery(TopAuctionsByProductNameQuery request, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(request.ProductName))
            { 
                return Array.Empty<TopAuctionsByProductName>();
            }
            var productName = request.ProductName.Trim();
            var canonicalName = new string(Product.CanonicalizeProductName(productName)
                .Where(c => char.IsLetterOrDigit(c)).ToArray()); //TODO prevent product name from containing invalid chars
            if (string.IsNullOrWhiteSpace(canonicalName)) //prevent selecting all items from db
            {
                return Array.Empty<TopAuctionsByProductName>();
            }
            var filter = Builders<TopAuctionsByProductName>.Filter.Regex(f => f.CanonicalName,
                new MongoDB.Bson.BsonRegularExpression(canonicalName)); 
            var topByProductName = await _dbContext.TopAuctionsByProductNameCollection
                .Find(filter)
                .Skip(request.Page * TopAuctionsInTagQuery.MAX_PER_PAGE)
                .Limit(TopAuctionsInTagQuery.MAX_PER_PAGE).ToListAsync();

            return topByProductName.ToArray();
        }
    }
}