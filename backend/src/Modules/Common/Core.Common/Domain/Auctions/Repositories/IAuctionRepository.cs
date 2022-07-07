using System;

namespace Core.Common.Domain.Auctions
{
    public interface IAuctionRepository
    {
        Auction FindAuction(Guid auctionId);
        Auction FindAuction(Guid auctionId, long version);
        Auction AddAuction(Auction auction);
        void RemoveAuction(Guid auctionId);
        void UpdateAuction(Auction auction);
    }
}