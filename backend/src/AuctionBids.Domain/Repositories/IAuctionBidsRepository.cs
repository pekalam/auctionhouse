using AuctionBids.Domain.Shared;

namespace AuctionBids.Domain.Repositories
{
    public interface IAuctionBidsRepository
    {
        AuctionBids WithAuctionId(AuctionId auctionId);
        void Add(AuctionBids auctionBids);
    }
}
