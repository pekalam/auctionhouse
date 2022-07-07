namespace Auctions.Domain.Services
{
    public interface IAuctionImageConversion
    {
        AuctionImageRepresentation ConvertTo(AuctionImageRepresentation imageRepresentation, AuctionImageSize size);
        bool ValidateImage(AuctionImageRepresentation imageRepresentation, string[] allowedExtensions);
    }
}