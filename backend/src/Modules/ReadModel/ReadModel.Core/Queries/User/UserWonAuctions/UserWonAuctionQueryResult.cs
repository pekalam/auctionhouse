using System.Collections.Generic;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.User.UserWonAuctions
{
    public class UserWonAuctionQueryResult
    {
        public List<AuctionRead> Auctions { get; set; }
        public long Total { get; set; }
    }
}