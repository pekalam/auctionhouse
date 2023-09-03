using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ReadModel.Contracts.Model
{
    public class UserRead
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }
        public UserIdentityRead UserIdentity { get; set; }
        public List<UserBid> UserBids { get; set; } = new List<UserBid>();
        public UserAddress Address { get; set; }
        [BsonDefaultValue(0)]
        [BsonRepresentation(BsonType.Decimal128)]
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal Credits { get; set; }

        public long Version { get; set; }
    }
}