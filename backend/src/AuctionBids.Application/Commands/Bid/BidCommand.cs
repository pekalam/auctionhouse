using System;
using Core.Command.Exceptions;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Users;

namespace Core.Command.Bid
{
    [AuthorizationRequired]
    public class BidCommand : ICommand
    {
        public Guid AuctionId { get; set; }
        public decimal Price { get; set; }

        [SignedInUser]
        public UserId SignedInUser { get; set; }

        public BidCommand(Guid auctionId, decimal price)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}");}
            AuctionId = auctionId;
            Price = price;
        }
    }
}
