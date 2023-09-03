using System;
using System.Collections.Generic;
using Common.Application.Queries;
using MongoDB.Bson.Serialization.Attributes;
using ReadModel.Contracts.Model;

namespace ReadModel.Contracts.Queries.Auction.MostViewed
{
    public class MostViewedAuctionsQuery : IQuery<IEnumerable<MostViewedAuctionsResult>>
    {
        public const int AUCTIONS_LIMIT = 10;
        public const int VIEWS_MIN = 2;
    }

    public class MostViewedAuctionsResult
    {
        public string AuctionId { get; set; }
        public string AuctionName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool BuyNowOnly { get; set; }
        [BsonDefaultValue(0)] public decimal BuyNowPrice { get; set; }
        [BsonDefaultValue(0)] public decimal ActualPrice { get; set; }
        public int TotalBids { get; set; }
        public int Views { get; set; }
        public AuctionImageRead[] AuctionImages { get; set; }
    }
}