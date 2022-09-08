using System;
using System.Collections.Generic;
using System.Linq;
using AuctionBids.Domain.Repositories;

namespace FunctionalTests.Mocks
{
    using AuctionBids.Domain;
    using AuctionBids.Domain.Shared;

    public class InMemoryAuctionBidsRepository : IAuctionBidsRepository
    {
        public static InMemoryAuctionBidsRepository Instance { get; } = new InMemoryAuctionBidsRepository();

        private readonly Dictionary<AuctionBidsId, AuctionBids> _auctions = new();

        public IReadOnlyList<AuctionBids> All => _auctions.Values.ToList();

        public void Add(AuctionBids auctionBids)
        {
            _auctions[auctionBids.AggregateId] = auctionBids;
        }

        void IAuctionBidsRepository.Update(AuctionBids auctionBids)
        {
            _auctions[auctionBids.AggregateId] = auctionBids;
        }

        public AuctionBids? WithAuctionId(AuctionId auctionId)
        {
            return _auctions.Values.FirstOrDefault(a => a.AuctionId.Value == auctionId.Value);
        }

        internal void Clear() => _auctions.Clear();
    }
}