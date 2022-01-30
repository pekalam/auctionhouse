using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReadModel.Core.Model
{
    public class AuctionRead
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }
        public string AuctionId { get; set; }

        public string Name { get; set; }
        public UserIdentityRead Owner { get; set; }
        public ProductRead Product { get; set; }
        public CategoryRead Category { get; set; }
        public AuctionImageRead[] AuctionImages { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime EndDate { get; set; }

        public bool BuyNowOnly { get; set; }

        [BsonDefaultValue(0)]
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal BuyNowPrice { get; set; }

        [BsonDefaultValue(0)]
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal ActualPrice { get; set; }
        public int TotalBids { get; set; }
        public int Views { get; set; }
        public string[] Tags { get; set; }

        [BsonDefaultValue(false)] public bool Completed { get; set; }
        [BsonDefaultValue(false)] public bool Canceled { get; set; }

        public bool Bought { get; set; }
        public UserIdentityRead Buyer { get; set; }
        public BidRead WinningBid { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime DateCreated { get; set; }

        [BsonDefaultValue(false)]
        public bool Archived { get; set; }

        [BsonDefaultValue(0)]
        [JsonIgnore]
        public long Version { get; set; }

        [JsonIgnore]
        public bool Locked { get; set; }
    }
}