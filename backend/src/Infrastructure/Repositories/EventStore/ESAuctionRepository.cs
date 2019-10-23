using System;
using Core.Common.Domain.Auctions;
using EventStore.ClientAPI;

namespace Infrastructure.Repositories.EventStore
{
    public class ESAuctionRepository : EventStoreRepositoryBase ,IAuctionRepository
    {
        public ESAuctionRepository(ESConnectionContext connectionContext) : base(connectionContext)
        {
        }

        protected override string GetStreamId(Guid auctionGuid) => $"{nameof(Auction)}-{auctionGuid}";

        public Auction FindAuction(Guid auctionId)
        {
            var events = ReadEvents(auctionId);
            if (events == null)
            {
                return null;
            }
            var auction = Auction.FromEvents(events);
            return auction;
        }

        public Auction FindAuction(Guid auctionId, long version)
        {
            var events = ReadEvents(auctionId, version - 1);
            if (events == null)
            {
                return null;
            }
            var auction = Auction.FromEvents(events);
            return auction;
        }

        public Auction AddAuction(Auction auction)
        {
            AppendPendingEventsToStream(auction.PendingEvents, ExpectedVersion.NoStream, auction.AggregateId);
            return auction;
        }

        public void RemoveAuction(Guid auctionId)
        {
            var result = _connectionContext.Connection
                .DeleteStreamAsync(GetStreamId(auctionId), ExpectedVersion.Any, hardDelete: true).Result;
        }

        public void UpdateAuction(Auction auction)
        {
            AppendPendingEventsToStream(auction.PendingEvents, auction.Version - auction.PendingEvents.Count - 1, auction.AggregateId);
        }
    }
}
