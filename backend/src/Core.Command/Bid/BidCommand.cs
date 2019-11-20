using System;
using System.ComponentModel.DataAnnotations;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Users;
using Core.Common.EventBus;

namespace Core.Command.Bid
{
    [AuthorizationRequired]
    public class BidCommand : ICommand
    {
        public Guid AuctionId { get; set; }
        public decimal Price { get; set; }
        [Required]
        public CorrelationId CorrelationId { get; set; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }

        public BidCommand(Guid auctionId, decimal price, CorrelationId correlationId)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}");}
            AuctionId = auctionId;
            Price = price;
            CorrelationId = correlationId;
        }
    }
}
