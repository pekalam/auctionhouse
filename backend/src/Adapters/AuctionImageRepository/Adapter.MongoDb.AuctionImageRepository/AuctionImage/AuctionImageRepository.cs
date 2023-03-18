using Auctions.Domain;
using Auctions.Domain.Repositories;
using Core.DomainFramework;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ReadModel.Core.Services;

namespace Adapter.MongoDb.AuctionImage
{
    internal class AuctionImageRepository : IAuctionImageRepository, IAuctionImageReadRepository
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
                _logger.LogError(e, "Cannot download image");
                throw new InfrastructureException("Cannot download image", e);
            }
        }

        private GridFSFileInfo? GetFileInfo(string imageId)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename,
                imageId);
            try
            {
                using var cursor = _dbContext.Bucket.Find(filter);
                var fileInfo = cursor.ToEnumerable().FirstOrDefault();
                return fileInfo;
            }
            catch (Exception e)
            {
                _logger.LogError($"Could not find FileInfo about image {imageId}");
                throw new InfrastructureException($"Could not find FileInfo about image {imageId}", e);
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
            UpdateResult result;
            try
            {
                result = _dbContext.Bucket.Database
                    .GetCollection<BsonDocument>("fs.files")
                    .UpdateOne(filter, update);
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"Could not update metadata of image {imageId}", e);
            }

            if (result.MatchedCount != 1)
            {
                throw new ArgumentException($"Matched count {result.MatchedCount}");
            }

            if (result.ModifiedCount != 1)
            {
                throw new ArgumentException($"Modified count {result.ModifiedCount}");
            }
        }

        public int UpdateManyMetadata(string[] imageIds, AuctionImageMetadata metadata)
        {
            if(imageIds.Length == 0)
            {
                return 0;
            }

            var filter = Builders<BsonDocument>.Filter.In("filename", imageIds);
            var update = Builders<BsonDocument>.Update.Set("metadata.IsAssignedToAuction", metadata.IsAssignedToAuction);

            UpdateResult result;
            try
            {
                result = _dbContext.Bucket.Database
            .GetCollection<BsonDocument>("fs.files")
            .UpdateMany(filter, update);
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"Could not update metadata of images", e);
            }

            if (result.MatchedCount <= 0)
            {
                throw new ArgumentException($"Matched count {result.MatchedCount}");
            }

            return (int)result.ModifiedCount;
        }

        public void Add(string imageId, AuctionImageRepresentation imageRepresentation)
        {
            if (Find(imageId) != null)
            {
                throw new ArgumentException($"{imageId} already exists");
            }
            var fileUploadOptions = new GridFSUploadOptions
            {
                Metadata = imageRepresentation.Metadata.ToBsonDocument()
            };
            try
            {
                var o = _dbContext.Bucket.UploadFromBytes(imageId, imageRepresentation.Img, fileUploadOptions);
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"Could not add image {imageId}", e);
            }
        }

        public void Remove(string imageId)
        {
            var info = GetFileInfo(imageId);
            if (info == null)
            {
                throw new ArgumentException($"Image {imageId} does not exist");
            }

            try
            {
                _dbContext.Bucket.Delete(info.Id);
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"Could not delete image {imageId}", e);
            }
        }
    }
}