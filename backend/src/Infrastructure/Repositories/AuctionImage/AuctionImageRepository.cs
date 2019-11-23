using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Common.Domain.Auctions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Infrastructure.Repositories.AuctionImage
{
    public class AuctionImageSizeConverterService : IAuctionImageSizeConverterService
    {
        private List<string[]> allowedFirstBytes = new List<string[]>();

        public AuctionImageSizeConverterService()
        {
            var jpg = new string[] { "FF", "D8" };
            var png = new string[] { "89", "50", "4E", "47", "0D", "0A", "1A", "0A" };
            allowedFirstBytes.Add(jpg);
            allowedFirstBytes.Add(png);
        }

        public AuctionImageRepresentation ConvertTo(AuctionImageRepresentation imageRepresentation,
            AuctionImageSize size)
        {
            return imageRepresentation;
        }

        public bool ValidateImage(AuctionImageRepresentation imageRepresentation, string[] allowedExtensions)
        {
            using (var stream = new MemoryStream(imageRepresentation.Img))
            {
                var read = new List<string>();

                for (int i = 0; i < 8; i++)
                {
                    var b = stream.ReadByte().ToString("X2");
                    read.Add(b);
                    bool isValid = allowedFirstBytes.Any(imgBytes => !imgBytes.Except(read).Any());
                    if (isValid)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    public class AuctionImageRepository : IAuctionImageRepository
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
            return new AuctionImageRepresentation()
            {
                Img = fileBytes,
                Metadata = metadata 
            };
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

        public void UpdateManyMetadata(string[] imageIds, AuctionImageMetadata metadata)
        {
            var filter = Builders<BsonDocument>.Filter.In("filename", imageIds);
            var update = Builders<BsonDocument>.Update.Set("metadata", metadata);

            var result = _dbContext.Bucket.Database
                .GetCollection<BsonDocument>("fs.files")
                .UpdateMany(filter, update);
            if (result.MatchedCount <= 0)
            {
                throw new Exception($"Matched count {result.MatchedCount}");
            }

            if (result.ModifiedCount <= 0)
            {
                throw new Exception($"Modified count {result.ModifiedCount}");
            }
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