namespace Core.Common.Domain.Auctions
{
    public interface IAuctionImageRepository
    {
        AuctionImageRepresentation FindImage(string imageId);
        void AddImage(string imageId, AuctionImageRepresentation imageRepresentation);
        void RemoveImage(string imageId);
    }
}