using AuctionBids.Domain.Repositories;
using AuctionBids.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    using AuctionBids.Domain;
    using Core.Common.Domain;

    internal class MsSqlAuctionBidsRepository : MsSqlESRepositoryBase, IAuctionBidsRepository
    {
        public MsSqlAuctionBidsRepository(MsSqlConnectionSettings connectionSettings) : base(connectionSettings)
        {
        }

        public void Add(AuctionBids auctionBids)
        {
            AddAggregate(auctionBids.PendingEvents, auctionBids.AggregateId.ToString()!, auctionBids.Version, "AuctionBids");
        }

        public AuctionBids? WithAuctionId(AuctionId auctionId)
        {
            List<Event>? events = ReadEvents(auctionId.Value);
            if (events == null)
            {
                return null;
            }
            return AuctionBids.FromEvents(events);
        }
    }
}
