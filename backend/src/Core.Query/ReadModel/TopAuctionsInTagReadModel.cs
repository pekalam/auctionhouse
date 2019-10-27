using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Query.ReadModel
{
    public class TopAuction
    {
        public string AuctionId { get; set; }
        public string AuctionName { get; set; }
    }

    public class TopAuctionsInTagReadModel
    {
        [BsonId]
        public string Tag { get; set; }
        public int Total { get; set; }
        public TopAuction[] Auctions { get; set; }
    }
}
