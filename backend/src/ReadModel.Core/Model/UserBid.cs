using Newtonsoft.Json;

namespace ReadModel.Core.Model
{
    public class UserBid
    {
        public string AuctionId { get; set; }
        public string AuctionName { get; set; }
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal Price { get; set; }
        public DateTime DateCreated { get; set; }
        public bool AuctionCanceled { get; set; }
        public bool AuctionCompleted { get; set; }
        public string BidId { get; set; }
        public bool BidCanceled { get; set; }
    }
}