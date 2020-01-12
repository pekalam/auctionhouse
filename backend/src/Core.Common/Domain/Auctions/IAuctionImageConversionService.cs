namespace Core.Common.Domain.Auctions
{
    public interface IAuctionImageConversionService
    {
        AuctionImageRepresentation ConvertTo(AuctionImageRepresentation imageRepresentation, AuctionImageSize size);
        bool ValidateImage(AuctionImageRepresentation imageRepresentation, string[] allowedExtensions);
    }
}