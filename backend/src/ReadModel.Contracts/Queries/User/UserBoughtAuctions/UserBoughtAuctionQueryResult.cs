﻿using System.Collections.Generic;
using ReadModel.Contracts.Model;

namespace ReadModel.Contracts.Queries.User.UserBoughtAuctions
{
    public class UserBoughtAuctionQueryResult
    {
        public List<AuctionRead> Auctions { get; set; }
        public long Total { get; set; }
    }
}