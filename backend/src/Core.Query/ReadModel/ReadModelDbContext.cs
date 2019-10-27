using System;
using Core.Common.Domain.Categories;
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

        public virtual IMongoCollection<AuctionReadModel> AuctionsReadModel =>
            _db.GetCollection<AuctionReadModel>("AuctionsReadModel");

        public virtual IMongoCollection<UserReadModel> UsersReadModel =>
            _db.GetCollection<UserReadModel>("UsersReadModel");

        public virtual IMongoCollection<TopAuctionsInTagReadModel> TagsAuctionsCollection =>
            _db.GetCollection<TopAuctionsInTagReadModel>("TopAuctionsInTag");
    }
}