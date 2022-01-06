using Auctions.Domain;
using Auctions.Domain.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Adapter.MongoDb.AuctionImage
{
    internal class AuctionImageRepository : IAuctionImageRepository
    {
        private readonly ImageDbContext _dbContext;
        private readonly ILogger<AuctionImageRepository> _logger;

        public AuctionImageRepository(ImageDbContext dbContext, ILogger<AuctionImageRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private byte[] GetFileFromDb(BsonObjectId objId)
        {
            try
            {
                var image = _dbContext.Bucket.DownloadAsBytes(objId);
                return image;
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot download image {e.Message}");
                throw;
            }
        }

        private GridFSFileInfo GetFileInfo(string imageId)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename,
                imageId);
            using (var cursor = _dbContext.Bucket.Find(filter))
            {
                var fileInfo = cursor.ToList().FirstOrDefault();
                return fileInfo;
            }
        }

        public AuctionImageRepresentation Find(string imageId)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename,
                imageId);
            var fileInfo = GetFileInfo(imageId);
            if (fileInfo == null)
            {
                return null;
            }
            var metadata = BsonSerializer.Deserialize<AuctionImageMetadata>(fileInfo.Metadata);
            var fileBytes = GetFileFromDb(fileInfo.Id);
            return new AuctionImageRepresentation(metadata, fileBytes);
        }

        public void UpdateMetadata(string imageId, AuctionImageMetadata metadata)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("filename", imageId);
            var update = Builders<BsonDocument>.Update.Set("metadata", metadata);
            var result = _dbContext.Bucket.Database
                .GetCollection<BsonDocument>("fs.files")
                .UpdateOne(filter, update);
            if (result.MatchedCount != 1)
            {
                throw new Exception($"Matched count {result.MatchedCount}");
            }

            if (result.ModifiedCount != 1)
            {
                throw new Exception($"Modified count {result.ModifiedCount}");
            }
        }

        public int UpdateManyMetadata(string[] imageIds, AuctionImageMetadata metadata)
        {
            var filter = Builders<BsonDocument>.Filter.In("filename", imageIds);
            var update = Builders<BsonDocument>.Update.Set("metadata.IsAssignedToAuction", metadata.IsAssignedToAuction);

            var result = _dbContext.Bucket.Database
                .GetCollection<BsonDocument>("fs.files")
                .UpdateMany(filter, update);
            if (result.MatchedCount <= 0)
            {
                throw new Exception($"Matched count {result.MatchedCount}");
            }

            return (int)result.ModifiedCount;
        }

        public void Add(string imageId, AuctionImageRepresentation imageRepresentation)
        {
            if (Find(imageId) == null)
            {
                var fileUploadOptions = new GridFSUploadOptions();
                fileUploadOptions.Metadata = imageRepresentation.Metadata.ToBsonDocument();
                var o = _dbContext.Bucket.UploadFromBytes(imageId, imageRepresentation.Img, fileUploadOptions);
            }
            else
            {
                throw new Exception($"{imageId} already exists");
            }
        }

        public void Remove(string imageId)
        {
            var info = GetFileInfo(imageId);
            if (info != null)
            {
                _dbContext.Bucket.Delete(info.Id);
            }
            else
            {
                throw new Exception($"Image {imageId} does not exist");
            }
        }
    }
}