using Auctions.Domain;

namespace Test.Auctions.Domain
{
    public class GivenAuctionImage
    {
        public AuctionImage Valid()
        {
            var imageId = Guid.NewGuid().ToString();
            return new(imageId + "_sz1", imageId + "_sz2", imageId + "_sz3"); ;
        }
    }
}
