using System;
using Core.Common.Domain.AuctionBids;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Core.Query.ReadModel
{
    public class DecimalRoundingConverter : JsonConverter<decimal>
    {
        public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
        {
            var rounded = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
            writer.WriteValue(rounded);
        }

        public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }



    public class BidRead
    {
        public string BidId { get; set; }
        public string AuctionId { get; set; }
        public UserIdentityRead UserIdentity { get; set; }
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal Price { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime DateCreated { get; set; }

        public BidRead(Bid bid)
        {
            BidId = bid.Id.ToString();
            //AuctionId = bid.AuctionId.ToString(); //TODO add id
            UserIdentity = new UserIdentityRead(bid.UserId, bid.UserId.ToString()); //TODO
            Price = bid.Price;
            DateCreated = bid.Date; //TODO rename
        }
    }

    public class AuctionRead
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId Id { get; set; }
        public string AuctionId { get; set; }

        public string Name { get; set; }
        public UserIdentityRead Owner { get; set; }
        public Product Product { get; set; }
        public Category Category { get; set; }
        public AuctionImage[] AuctionImages { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime EndDate { get; set; }

        public bool BuyNowOnly { get; set; }
        [BsonDefaultValue(0)]
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal BuyNowPrice { get; set; }
        [BsonDefaultValue(0)]
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal ActualPrice { get; set; }
        public int TotalBids { get; set; }
        public int Views { get; set; }
        public string[] Tags { get; set; }

        [BsonDefaultValue(false)] public bool Completed { get; set; }
        [BsonDefaultValue(false)] public bool Canceled { get; set; }
        public bool Bought { get; set; }
        public UserIdentityRead Buyer { get; set; }
        public BidRead WinningBid { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime DateCreated { get; set; }
        [BsonDefaultValue(false)]
        public bool Archived { get; set; }

        [BsonDefaultValue(0)]
        [JsonIgnore]
        public long Version { get; set; }
    }
}