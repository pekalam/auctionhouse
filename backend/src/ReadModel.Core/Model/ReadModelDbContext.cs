﻿using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using ReadModel.Core.Views;

namespace ReadModel.Core.Model
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

        public ReadModelDbContext(MongoDbSettings options)
        {
            //BsonSerializer.RegisterSerializer(typeof(Category), new CategorySerializer(categoryBuilder));
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
            _db.GetCollection<TopAuctionsInTag>(nameof(TopAuctionsInTag));

        public virtual IMongoCollection<TopAuctionsByProductName> TopAuctionsByProductNameCollection =>
            _db.GetCollection<TopAuctionsByProductName>(nameof(TopAuctionsByProductName));

        public virtual IMongoCollection<CommonTags> CommonTagsCollection =>
            _db.GetCollection<CommonTags>(nameof(CommonTags));


        public virtual IMongoCollection<EndingAuctions> EndingAuctionsCollection =>
            _db.GetCollection<EndingAuctions>(nameof(EndingAuctions));

    }
}