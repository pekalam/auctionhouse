using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Query.ReadModel
{
    public class TagAuctions
    {
        public string AuctionId { get; set; }
        public string AuctionName { get; set; }
    }

    public class TagsAuctionsReadModel
    {
        [BsonId]
        public string Tag { get; set; }
        public int Total { get; set; }
        public TagAuctions[] Auctions { get; set; }
    }
}
