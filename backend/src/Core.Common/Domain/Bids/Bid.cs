using System;
using Core.Common.Exceptions;

namespace Core.Common.Domain.Bids
{
    public class Bid
    {
        public Guid BidId { get; set; }
        public Guid AuctionId { get; set; }
        public Guid UserId { get; set; }
        public decimal Price { get; set; }

        //TODO
        public DateTime DateCreated { get; }

        public Bid(Guid auctionId, Guid userId, decimal price, DateTime dateCreated)
        {
            if (dateCreated.Kind != DateTimeKind.Utc)
            {
                throw new DomainException("Invalid date format");
            }
            if (userId == Guid.Empty)
            {
                throw new DomainException("Invalid user");
            }
            if (price <= 0)
            {
                throw new DomainException("Bid price is too low");
            }

            BidId = Guid.NewGuid();
            AuctionId = auctionId;
            UserId = userId;
            Price = price;
            DateCreated = dateCreated;
        }
    }
}