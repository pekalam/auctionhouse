using System;
using System.Collections.Generic;
using Core.Common.Domain.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Query.ReadModel
{
    public class UserBid
    {
        public string AuctionId { get; set; }
        public decimal Price { get; set; }
        public DateTime DateCreated { get; set; }
        public bool AuctionCanceled { get; set; }
        public bool AuctionCompleted { get; set; }

    }

    public class UserAddress
    {
        public string City { get; set; }
        public string Street { get; set; }
    }

    public class UserIdentityReadModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public UserIdentityReadModel()
        {
            
        }

        public UserIdentityReadModel(UserIdentity userIdentity)
        {
            UserId = userIdentity.UserId.ToString();
            UserName = userIdentity.UserName;
        }
    }

    public class UserReadModel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public UserIdentityReadModel UserIdentity { get; set; }
        public ICollection<string> CreatedAuctions { get; set; } = new HashSet<string>();
        public List<UserBid> UserBids { get; set; } = new List<UserBid>();
        public UserAddress Address { get; set; }
    }
}