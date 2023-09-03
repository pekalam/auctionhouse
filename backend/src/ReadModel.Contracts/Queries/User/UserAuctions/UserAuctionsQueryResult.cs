using System.Collections.Generic;
using ReadModel.Contracts.Model;

namespace ReadModel.Contracts.Queries.User.UserAuctions
{
    public class UserAuctionsQueryResult
    {
        private static readonly AuctionRead[] _default = new AuctionRead[0];

        public IEnumerable<AuctionRead> Auctions { get; set; } = _default;
        public long Total { get; set; } = 0;
    }
}
