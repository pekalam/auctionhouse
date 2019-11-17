using System;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Bids;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Query.ReadModel
{
    public class BidRead
    {
        public string BidId { get; set; }
        public string AuctionId { get; set; }
        public UserIdentityRead UserIdentity { get; set; }
        public decimal Price { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime DateCreated { get; }

        public BidRead(Bid bid)
        {
            BidId = bid.BidId.ToString();
            AuctionId = bid.AuctionId.ToString();
            UserIdentity = new UserIdentityRead(bid.UserIdentity);
            Price = bid.Price;
            DateCreated = bid.DateCreated;
        }
    }

    public class AuctionRead
    {
        [BsonId] public ObjectId Id { get; set; }
        public string AuctionId { get; set; }


        public UserIdentity Creator { get; set; }
        public Product Product { get; set; }
        public Category Category { get; set; }
        public AuctionImage[] AuctionImages { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime EndDate { get; set; }

        public bool BuyNowOnly { get; set; }
        [BsonDefaultValue(0)] public decimal BuyNowPrice { get; set; }
        [BsonDefaultValue(0)] public decimal ActualPrice { get; set; }
        public int TotalBids { get; set; }
        public int Views { get; set; }
        public string[] Tags { get; set; }

        [BsonDefaultValue(false)] public bool Completed { get; set; }
        [BsonDefaultValue(false)] public bool Canceled { get; set; }
        public bool Bought { get; set; }
        public UserIdentity Buyer { get; set; }
        public BidRead WinningBid { get; set; }

        [BsonDefaultValue(0)] public long Version { get; set; }
    }
}