using System;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Query.Queries.Auction.Auctions
{
    public class AuctionsQueryResult
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string AuctionId { get; set; }
        public UserIdentity Creator { get; set; }
        public string ProductName { get; set; }
        public Category Category { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EndDate { get; set; }
        public decimal? BuyNowPrice { get; set; }
        public decimal? ActualPrice { get; set; }
        public Common.Domain.Auctions.AuctionImage[] AuctionImages { get; set; }
    }
}
