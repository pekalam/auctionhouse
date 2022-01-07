using Auctions.Domain;
using Auctions.Domain.Repositories;
using Core.Common.Domain;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    public class MsSqlConnectionSettings
    {
        public string ConnectionString { get; set; }
    }

    internal class MsSqlAuctionRepository : MsSqlESRepositoryBaseExceptionDecorator, IAuctionRepository
    {
        public MsSqlAuctionRepository(MsSqlConnectionSettings connectionSettings) : base(connectionSettings)
        {
        }

        public Auction FindAuction(Guid auctionId)
        {
            List<Event>? aggEvents = ReadEvents(auctionId);
            var auction = aggEvents != null ? Auction.FromEvents(aggEvents) : null;
            return auction;
        }

        public Auction FindAuction(Guid auctionId, long version)
        {
            List<Event>? aggEvents = ReadEvents(auctionId, version);
            var auction = aggEvents != null ? Auction.FromEvents(aggEvents) : null;
            return auction;
        }

        public Auction AddAuction(Auction auction)
        {
            AddAggregate(auction.PendingEvents, auction.AggregateId.ToString(), auction.Version, "Auction");
            return auction;
        }

        public void RemoveAuction(Guid auctionId)
        {
            RemoveAggregate(auctionId.ToString());
        }

        public void UpdateAuction(Auction auction)
        {
            UpdateAggregate(auction.PendingEvents, auction.AggregateId.ToString(), auction.Version, "Auction");
        }
    }
}
