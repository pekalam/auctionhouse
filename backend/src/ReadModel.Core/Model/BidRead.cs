using System;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace ReadModel.Core.Model
{
    public class BidRead
    {
        public string BidId { get; set; }
        public string AuctionId { get; set; }
        public UserIdentityRead UserIdentity { get; set; }
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal Price { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime DateCreated { get; set; }
    }

    public class AuctionBidsRead
    {
        public string AuctionId { get; set; }
        public string OwnerId { get; set; }
        public string? WinnerId { get; set; }
        public string? WinnerBidId { get; set; }
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal CurrentPrice { get; set; }
    }
}