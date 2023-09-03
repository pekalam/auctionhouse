using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ReadModel.Contracts.Model
{
    public class UserPaymentsRead
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
    }
}
