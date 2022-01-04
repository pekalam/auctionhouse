using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ReadModel.Core.Views
{
    public class EndingAuctions
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }
        public string AuctionId { get; set; }
        public string Name { get; set; }
        public DateTime EndDate { get; set; }
        [BsonDefaultValue(0)] public decimal ActualPrice { get; set; }
        public int TotalBids { get; set; }
        public Common.Domain.Auctions.AuctionImage[] AuctionImages { get; set; }
        public string[] Tags { get; set; }
        public double MinToEnd { get; set; }
        public int Views { get; set; }
    }
}
