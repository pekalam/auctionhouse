﻿using Auctions.Domain.Repositories;
using Core.DomainFramework;

namespace Auctions.Domain.Services
{
    public class AuctionImageService
    {
        private readonly IAuctionImageRepository _imageRepository;
        private readonly IAuctionImageConversion _imageConverterService;


        public AuctionImageService(IAuctionImageRepository imageRepository, IAuctionImageConversion imageConverterService)
        {
            _imageRepository = imageRepository;
            _imageConverterService = imageConverterService;
        }

        private void AddConvertedImage(string imageId, AuctionImageSize size, AuctionImageRepresentation imgRepresentation)
        {
            AuctionImageRepresentation converted = null!;
            try
            {
                converted = _imageConverterService.ConvertTo(imgRepresentation, size);
            }
            catch (Exception ex)
            {
                throw new DomainException("Cannot convert image", ex);
            }

            try
            {
                _imageRepository.Add(imageId, converted);
            }
            catch (Exception ex)
            {
                throw new DomainException("Cannot add image", ex);
            }
        }

        public AuctionImage AddAuctionImage(AuctionImageRepresentation representation)
        {
            if (!_imageConverterService.ValidateImage(representation, AuctionImage.AllowedExtensions))
            {
                throw new DomainException("Invalid image");
            }

            var img = new AuctionImage(
                AuctionImage.GenerateImageId(AuctionImageSize.SIZE1),
                AuctionImage.GenerateImageId(AuctionImageSize.SIZE2),
                AuctionImage.GenerateImageId(AuctionImageSize.SIZE3)
            );

            AddConvertedImage(img.Size1Id, AuctionImageSize.SIZE1, representation);
            AddConvertedImage(img.Size2Id, AuctionImageSize.SIZE2, representation);
            AddConvertedImage(img.Size3Id, AuctionImageSize.SIZE3, representation);

            return img;
        }

        public void RemoveAuctionImage(AuctionImage img)
        {
            _imageRepository.Remove(img.Size1Id);
            _imageRepository.Remove(img.Size2Id);
            _imageRepository.Remove(img.Size3Id);
        }
    }
}
