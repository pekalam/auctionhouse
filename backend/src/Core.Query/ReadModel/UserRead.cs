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
        public string AuctionName { get; set; }
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
        public ObjectId Id { get; set; }
        public UserIdentityRead UserIdentity { get; set; }
        public List<string> CreatedAuctions { get; set; } =
            new List<string>();
        public List<string> BoughtAuctions { get; set; } = new List<string>();
        public List<string> WonAuctions { get; set; } = new List<string>();
        public List<UserBid> UserBids { get; set; } = new List<UserBid>();
        public UserAddress Address { get; set; }
        [BsonDefaultValue(0)]
        public decimal Credits { get; set; }
    }
}