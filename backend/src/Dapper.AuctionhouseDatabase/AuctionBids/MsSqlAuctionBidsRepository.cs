using AuctionBids.Domain.Repositories;
using AuctionBids.Domain.Shared;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    using AuctionBids.Domain;
    using Core.Common.Domain;

    internal class MsSqlAuctionBidsRepository : MsSqlESRepositoryBaseExceptionDecorator, IAuctionBidsRepository
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
