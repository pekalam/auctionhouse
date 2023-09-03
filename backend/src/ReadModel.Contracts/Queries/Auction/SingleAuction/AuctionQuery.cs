using Common.Application.Queries;
using ReadModel.Contracts.Model;

namespace ReadModel.Contracts.Queries.Auction.SingleAuction
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
