using System;
using Core.Common.Domain.Categories;
using Core.Query.Views;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Core.Query.ReadModel
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public class ReadModelDbContext
    {
        private readonly IMongoDatabase _db;
        private readonly MongoClient _client;

        public ReadModelDbContext(MongoDbSettings options, CategoryBuilder categoryBuilder)
        {
            BsonSerializer.RegisterSerializer(typeof(Category), new CategorySerializer(categoryBuilder));
            if (BsonSerializer.SerializerRegistry.GetSerializer(typeof(Guid)) == null)
            {
                BsonSerializer.RegisterSerializer(typeof(Guid),
                    new GuidSerializer(BsonType.String));
            }
            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));

            _client = new MongoClient(new MongoUrl(options.ConnectionString));
            _db = _client.GetDatabase(options.DatabaseName);
        }

        public virtual IMongoClient Client => _client;

        public virtual IMongoCollection<AuctionRead> AuctionsReadModel =>
            _db.GetCollection<AuctionRead>("AuctionsReadModel");

        public virtual IMongoCollection<UserRead> UsersReadModel =>
            _db.GetCollection<UserRead>("UsersReadModel");

        public virtual IMongoCollection<TopAuctionsInTag> TagsAuctionsCollection =>
            _db.GetCollection<TopAuctionsInTag>("TopAuctionsInTag");

        public virtual IMongoCollection<TopAuctionsByProductName> TopAuctionsByProductName =>
            _db.GetCollection<TopAuctionsByProductName>("TopAuctionsByProductName");

        public virtual IMongoCollection<CommonTags> CommonTagsCollection =>
            _db.GetCollection<CommonTags>("CommonTags");
        
    }
}