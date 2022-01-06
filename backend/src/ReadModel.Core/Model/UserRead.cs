using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ReadModel.Core.Model
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
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal Credits { get; set; }
    }
}