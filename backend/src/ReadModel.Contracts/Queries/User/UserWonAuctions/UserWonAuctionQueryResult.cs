using System.Collections.Generic;
using ReadModel.Contracts.Model;

namespace ReadModel.Contracts.Queries.User.UserWonAuctions
{
    public class UserWonAuctionQueryResult
    {
        public List<AuctionRead> Auctions { get; set; }
        public long Total { get; set; }
    }
}