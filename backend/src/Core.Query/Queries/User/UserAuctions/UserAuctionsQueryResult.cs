using System.Collections.Generic;
using Core.Query.ReadModel;

namespace Core.Query.Queries.User.UserAuctions
{
    public class UserAuctionsQueryResult
    {
        private static AuctionRead[] _auctions = new AuctionRead[0];

        public IEnumerable<AuctionRead> Auctions { get; set; } = _auctions;
    }
}
