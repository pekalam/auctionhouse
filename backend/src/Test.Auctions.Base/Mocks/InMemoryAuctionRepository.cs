using Auctions.Domain;
using Auctions.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctions.Tests.Base.Mocks
{
    public class InMemoryAuctionRepository : IAuctionRepository
    {
        private readonly Dictionary<AuctionId, Auction> _auctions = new();

        public IReadOnlyList<Auction> All => _auctions.Values.ToList();

        public Auction AddAuction(Auction auction)
        {
            _auctions[auction.AggregateId] = auction;
            return auction;
        }

        public Auction FindAuction(Guid auctionId)
        {
            if (!_auctions.ContainsKey(new(auctionId)))
            {
                return null;
            }
            return _auctions[new(auctionId)];
        }

        public Auction FindAuction(Guid auctionId, long version)
        {
            return _auctions[new(auctionId)];
        }

        public void RemoveAuction(Guid auctionId)
        {
            _auctions.Remove(new(auctionId));
        }

        public void UpdateAuction(Auction auction)
        {
            _auctions[auction.AggregateId] = auction;
        }
    }
}
