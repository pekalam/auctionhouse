using System.Collections.Generic;
using Core.Query.ReadModel;

namespace Core.Query.Queries.User.UserWonAuctions
{
    public class UserWonAuctionQueryResult
    {
        public List<AuctionRead> Auctions { get; set; }
        public long Total { get; set; }
    }
}