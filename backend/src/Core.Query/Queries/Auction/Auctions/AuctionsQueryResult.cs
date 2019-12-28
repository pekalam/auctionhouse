using System;
using System.Collections.Generic;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Query.ReadModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Query.Queries.Auction.Auctions
{
    public class AuctionListItem
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string AuctionId { get; set; }
        public UserIdentityRead Creator { get; set; }
        public string ProductName { get; set; }
        public string Name { get; set; }
        public Condition Condition { get; set; }
        public Category Category { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EndDate { get; set; }
        public decimal BuyNowPrice { get; set; }
        public decimal ActualPrice { get; set; }
        public bool BuyNowOnly { get; set; }
        public int TotalBids { get; set; }
        public Common.Domain.Auctions.AuctionImage[] AuctionImages { get; set; }

    }

    public class AuctionsQueryResult
    {
        public IEnumerable<AuctionListItem> Auctions { get; set; }
        public long Total { get; set; }
    }
}
