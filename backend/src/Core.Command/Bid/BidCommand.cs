using System;
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
        public CorrelationId CorrelationId { get; set; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }

        public BidCommand(Guid auctionId, decimal price, CorrelationId correlationId)
        {
            AuctionId = auctionId;
            Price = price;
            CorrelationId = correlationId;
        }
    }
}
