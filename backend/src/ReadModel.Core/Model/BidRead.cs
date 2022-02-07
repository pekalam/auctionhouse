using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReadModel.Core.Model
{
    public class BidRead
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }
        public string BidId { get; set; }
        public string AuctionId { get; set; }
        public UserIdentityRead UserIdentity { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal Price { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime DateCreated { get; set; }
    }

    public class AuctionBidsRead
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }
        public string AuctionBidsId { get; set; }
        public string AuctionId { get; set; }
        public string OwnerId { get; set; }
        public string? WinnerId { get; set; }
        public string? WinnerBidId { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal CurrentPrice { get; set; }
        [JsonIgnore]
        public long Version { get; set; }
    }
}