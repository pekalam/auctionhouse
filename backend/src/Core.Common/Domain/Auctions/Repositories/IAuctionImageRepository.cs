namespace Core.Common.Domain.Auctions
{
    public interface IAuctionImageRepository
    {
        AuctionImageRepresentation Find(string imageId);
        void Add(string imageId, AuctionImageRepresentation imageRepresentation);
        void Remove(string imageId);
        void UpdateMetadata(string imageId, AuctionImageMetadata metadata);
        void UpdateManyMetadata(string[] imageIds, AuctionImageMetadata Metadata);
    }
}