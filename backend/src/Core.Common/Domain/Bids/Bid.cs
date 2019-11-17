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

        //TODO
        public DateTime DateCreated { get; }

        public Bid(Guid auctionId, UserIdentity userIdentity, decimal price, DateTime dateCreated)
        {
            if (dateCreated.Kind != DateTimeKind.Utc)
            {
                throw new DomainException("Invalid date format");
            }
            if (userIdentity == null || userIdentity == UserIdentity.Empty)
            {
                throw new DomainException("Invalid user");
            }
            if (price <= 0)
            {
                throw new DomainException("Bid price is too low");
            }

            BidId = Guid.NewGuid();
            AuctionId = auctionId;
            UserIdentity = userIdentity;
            Price = price;
            DateCreated = dateCreated;
        }
    }
}