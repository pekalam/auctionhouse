using Common.Application.Queries;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.SingleAuction
{
    public class AuctionQuery : IQuery<AuctionRead>
    {
        public string AuctionId { get; }

        public AuctionQuery(string auctionId)
        {
            AuctionId = auctionId;
        }
    }
}
