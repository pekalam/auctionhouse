using System.Collections.Generic;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.User.UserAuctions
{
    public class UserAuctionsQueryResult
    {
        private static readonly AuctionRead[] _default = new AuctionRead[0];

        public IEnumerable<AuctionRead> Auctions { get; set; } = _default;
        public long Total { get; set; } = 0;
    }
}
