using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadModel.Core.Model
{
    public class UserPaymentsRead
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
    }
}
