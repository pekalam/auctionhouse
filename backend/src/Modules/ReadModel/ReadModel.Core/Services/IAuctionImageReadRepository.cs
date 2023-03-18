using Auctions.Domain;

namespace ReadModel.Core.Services;

public interface IAuctionImageReadRepository
{
    AuctionImageRepresentation Find(string imageId);
}
