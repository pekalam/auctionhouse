using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Query.Views.TopAuctionsByProductName
{

    public class TopAuctionsByProductName
    {
        [BsonId]
        public string CanonicalName { get; set; }
        public int Total { get; set; }
        public TopAuction[] Auctions { get; set; }
    }
}
