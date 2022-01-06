namespace Auctions.Domain.Repositories
{
    public interface IAuctionImageRepository
    {
        AuctionImageRepresentation Find(string imageId);
        void Add(string imageId, AuctionImageRepresentation imageRepresentation);
        void Remove(string imageId);
        void UpdateMetadata(string imageId, AuctionImageMetadata metadata);
        int UpdateManyMetadata(string[] imageIds, AuctionImageMetadata Metadata);
    }
}