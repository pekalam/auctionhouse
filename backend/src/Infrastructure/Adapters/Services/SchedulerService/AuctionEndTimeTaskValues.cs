using System;
using Core.Common.Domain.Auctions;

namespace Infrastructure.Adapters.Services.SchedulerService
{
    public class AuctionEndTimeTaskValues
    {
        public Guid AuctionId { get; set; }

        public AuctionEndTimeTaskValues(Auction auction)
        {
            AuctionId = auction.AggregateId;
        }
    }
}