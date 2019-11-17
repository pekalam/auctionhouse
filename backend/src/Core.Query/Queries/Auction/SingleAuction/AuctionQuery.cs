using Core.Query.ReadModel;
using MediatR;

namespace Core.Query.Queries.Auction.SingleAuction
{
    public class AuctionQuery : IRequest<AuctionRead>
    {
        public string AuctionId { get; }

        public AuctionQuery(string auctionId)
        {
            AuctionId = auctionId;
        }
    }
}
