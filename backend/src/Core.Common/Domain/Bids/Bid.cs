using System;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Bids
{
    public class Bid
    {
        public Guid BidId { get; set; }
        public Guid AuctionId { get; set; }
        public UserIdentity UserIdentity { get; set; }
        public decimal Price { get; set; }
        public DateTime DateCreated { get; }

        public Bid(Guid auctionId, UserIdentity userIdentity, decimal price)
        {
            BidId = Guid.NewGuid();
            AuctionId = auctionId;
            UserIdentity = userIdentity;
            Price = price;
            DateCreated = DateTime.UtcNow;
        }

        public Bid()
        {
            
        }
    }
}