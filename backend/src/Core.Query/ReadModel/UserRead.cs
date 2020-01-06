using System;
using System.Collections.Generic;
using Core.Common.Domain.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Core.Query.ReadModel
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

    public class UserAddress
    {
        public string City { get; set; }
        public string Street { get; set; }
    }

    public class UserIdentityRead
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public UserIdentityRead()
        {
            
        }

        public UserIdentityRead(UserIdentity userIdentity)
        {
            UserId = userIdentity.UserId.ToString();
            UserName = userIdentity.UserName;
        }
    }

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