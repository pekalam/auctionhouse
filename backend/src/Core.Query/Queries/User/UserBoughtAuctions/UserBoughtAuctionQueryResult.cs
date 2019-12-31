using System.Collections.Generic;
using Core.Query.ReadModel;

namespace Core.Query.Queries.User.UserAuctions
{
    public class UserBoughtAuctionQueryResult
    {
        public List<AuctionRead> Auctions { get; set; }
        public long Total { get; set; }
    }
}