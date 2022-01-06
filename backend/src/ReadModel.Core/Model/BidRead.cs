using System;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

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
}