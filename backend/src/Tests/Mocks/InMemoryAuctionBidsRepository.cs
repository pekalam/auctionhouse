using System;
using System.Collections.Generic;
using System.Linq;
using AuctionBids.Domain.Repositories;

namespace FunctionalTests.Mocks
{
    using AuctionBids.Domain;
    using AuctionBidsAuctionId = AuctionBids.Domain.Shared.AuctionId;

    public class InMemoryAuctionBidsRepository : IAuctionBidsRepository
    {
        private readonly Dictionary<AuctionBidsId, AuctionBids> _auctions = new();

        public IReadOnlyList<AuctionBids> All => _auctions.Values.ToList();

        public void Add(AuctionBids auctionBids)
        {
            _auctions[auctionBids.AggregateId] = auctionBids;
        }


        public AuctionBids WithAuctionId(AuctionBidsAuctionId auctionId)
        {
            throw new NotImplementedException();
        }
    }
}