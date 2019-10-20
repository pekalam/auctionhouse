using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation.Internal;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Infrastructure.Adapters.Repositories.AuctionImage
{
    public class ImageDbContext
    {
        private readonly IMongoDatabase _db;
        private readonly MongoClient _client;
        private GridFSBucket _bucket;

        public ImageDbContext(ImageDbSettings options)
        {
            if (BsonSerializer.SerializerRegistry.GetSerializer(typeof(Guid)) == null)
            {
                BsonSerializer.RegisterSerializer(typeof(Guid),
                    new GuidSerializer(BsonType.String));
            }

            _client = new MongoClient(new MongoUrl(options.ConnectionString));
            _db = _client.GetDatabase(options.DatabaseName);
            _bucket = new GridFSBucket(_db);
        }

        public GridFSBucket Bucket => _bucket;
    }

}