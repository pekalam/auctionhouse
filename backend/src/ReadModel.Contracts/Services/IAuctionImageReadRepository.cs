using Auctions.Domain;

namespace ReadModel.Contracts.Services;

public interface IAuctionImageReadRepository
{
    AuctionImageRepresentation Find(string imageId);
}
