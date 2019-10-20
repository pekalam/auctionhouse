using System;
using System.Linq;
using Core.Common.Domain.Auctions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Infrastructure.Adapters.Repositories.AuctionImage
{
    public class AuctionImageSizeConverterService : IAuctionImageSizeConverterService
    {
        public AuctionImageRepresentation ConvertTo(AuctionImageRepresentation imageRepresentation,
            AuctionImageSize size)
        {
            return imageRepresentation;
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

        public AuctionImageRepresentation FindImage(string imageId)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename,
                imageId);
            var fileInfo = GetFileInfo(imageId);
            if (fileInfo == null)
            {
                return null;
            }
            var fileBytes = GetFileFromDb(fileInfo.Id);
            return new AuctionImageRepresentation()
            {
                Img = fileBytes
            };
        }

        public void AddImage(string imageId, AuctionImageRepresentation imageRepresentation)
        {
            if (FindImage(imageId) == null)
            {
                var o = _dbContext.Bucket.UploadFromBytes(imageId, imageRepresentation.Img);
            }
            else
            {
                throw new Exception($"{imageId} already exists");
            }
        }

        public void RemoveImage(string imageId)
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