using System;
using JetBrains.Annotations;

namespace Core.Common.Domain.Auctions
{
    public interface IAuctionRepository
    {
        [CanBeNull]
        Auction FindAuction(Guid auctionId);
        [CanBeNull]
        Auction FindAuction(Guid auctionId, long version);
        Auction AddAuction(Auction auction);
        void RemoveAuction(Guid auctionId);
        void UpdateAuction(Auction auction);
    }
}