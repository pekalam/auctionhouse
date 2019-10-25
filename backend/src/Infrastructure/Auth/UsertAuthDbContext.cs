using System;
using Core.Common.Auth;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Infrastructure.Auth
{
    public class UserAuthDbContextOptions
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public class UserAuthenticationDataMongo
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    internal static class UserAuthenticationDataAssembler
    {
        public static UserAuthenticationData From(UserAuthenticationDataMongo auth) => auth != null ? new UserAuthenticationData(){UserId = auth.UserId, UserName = auth.UserName, Password = auth.Password} : null;
        public static UserAuthenticationDataMongo From(UserAuthenticationData auth) => auth != null ? new UserAuthenticationDataMongo() { UserId = auth.UserId, UserName = auth.UserName, Password = auth.Password }: null;
    }

    public class UsertAuthDbContext
    {
        private readonly IMongoDatabase _db;
        private readonly MongoClient _client;

        public UsertAuthDbContext(UserAuthDbContextOptions options)
        {
            if (BsonSerializer.SerializerRegistry.GetSerializer(typeof(Guid)) == null)
            {
                BsonSerializer.RegisterSerializer(typeof(Guid),
                    new GuidSerializer(BsonType.String));
            }

            _client = new MongoClient(new MongoUrl(options.ConnectionString));
            _db = _client.GetDatabase(options.DatabaseName);
        }

        public virtual IMongoClient Client => _client;

        public virtual IMongoCollection<UserAuthenticationDataMongo> UserAuth =>
            _db.GetCollection<UserAuthenticationDataMongo>("UserAuthData");
    }
}
