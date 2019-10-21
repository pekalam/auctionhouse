using System;
using Core.Common.EventBus;
using Core.Common.Interfaces;
using MediatR;

namespace Core.Command.Bid
{
    public class BidCommand : IRequest, ICommand
    {
        public Guid AuctionId { get; set; }
        public decimal Price { get; set; }
        public CorrelationId CorrelationId { get; set; }

        public BidCommand(Guid auctionId, decimal price, CorrelationId correlationId)
        {
            AuctionId = auctionId;
            Price = price;
            CorrelationId = correlationId;
        }
    }
}
