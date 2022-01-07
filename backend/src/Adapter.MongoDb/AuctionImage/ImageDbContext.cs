using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Adapter.MongoDb.AuctionImage
{
    internal class ImageDbContext
    {
        private readonly IMongoDatabase _db;
        private readonly MongoClient _client;
        private readonly GridFSBucket _bucket;

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