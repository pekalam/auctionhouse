using System.ComponentModel.DataAnnotations;
using Core.Common.Query;
using Core.Query.ReadModel;

namespace Core.Query.Queries.Auction.SingleAuction
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
