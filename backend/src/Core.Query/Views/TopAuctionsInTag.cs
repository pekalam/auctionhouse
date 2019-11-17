using MongoDB.Bson.Serialization.Attributes;

namespace Core.Query.Views
{
    public class TopAuction
    {
        public string AuctionId { get; set; }
        public string AuctionName { get; set; }
    }

    public class TopAuctionsInTag
    {
        [BsonId]
        public string Tag { get; set; }
        public int Total { get; set; }
        public TopAuction[] Auctions { get; set; }
    }
}
