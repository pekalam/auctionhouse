﻿using System;
using System.Collections.Generic;
using Auctions.Domain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using ReadModel.Contracts.Model;

namespace ReadModel.Contracts.Queries.Auction.Auctions
{
    public class AuctionListItem
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string AuctionId { get; set; }
        public UserIdentityRead Owner { get; set; }
        public string ProductName { get; set; }
        public string Name { get; set; }
        public int Condition { get; set; }
        public CategoryRead Category { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EndDate { get; set; }
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal BuyNowPrice { get; set; }
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal ActualPrice { get; set; }
        public bool BuyNowOnly { get; set; }
        public int TotalBids { get; set; }
        public AuctionImageRead[] AuctionImages { get; set; }

    }

    public class AuctionsQueryResult
    {
        public IEnumerable<AuctionListItem> Auctions { get; set; }
        public long Total { get; set; }
    }
}
