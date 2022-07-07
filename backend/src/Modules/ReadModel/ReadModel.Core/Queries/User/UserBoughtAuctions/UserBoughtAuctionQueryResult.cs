using System.Collections.Generic;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.User.UserBoughtAuctions
{
    public class UserBoughtAuctionQueryResult
    {
        public List<AuctionRead> Auctions { get; set; }
        public long Total { get; set; }
    }
}