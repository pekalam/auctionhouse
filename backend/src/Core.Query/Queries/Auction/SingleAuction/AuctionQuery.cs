using Core.Query.ReadModel;
using MediatR;

namespace Core.Query.Queries.Auction.SingleAuction
{
    public class AuctionQuery : IRequest<AuctionReadModel>
    {
        public string AuctionId { get; }

        public AuctionQuery(string auctionId)
        {
            AuctionId = auctionId;
        }
    }
}
