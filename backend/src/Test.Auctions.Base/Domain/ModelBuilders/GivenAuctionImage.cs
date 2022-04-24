using Auctions.Domain;

namespace Auctions.Tests.Base.Domain.ModelBuilders
{
    public class GivenAuctionImage
    {
        public AuctionImage Build()
        {
            var imageId = Guid.NewGuid().ToString();
            return new(imageId + "_sz1", imageId + "_sz2", imageId + "_sz3"); ;
        }
    }
}
