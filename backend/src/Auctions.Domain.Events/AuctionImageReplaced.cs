using Core.Common.Domain;

namespace Auctions.DomainEvents
{
    public class AuctionImageReplaced : AuctionEvent
    {
        public Guid AuctionId { get; }
        public int ImgNum { get; }

        public string ImageSize1Id { get; }
        public string ImageSize2Id { get; }
        public string ImageSize3Id { get; }
        public Guid AuctionOwner { get; }

        public AuctionImageReplaced(Guid auctionId, int imgNum, Guid auctionOwner, string imageSize1Id, string imageSize2Id, string imageSize3Id) : base(EventNames.AuctionImageReplaced)
        {
            AuctionId = auctionId;
            ImgNum = imgNum;
            AuctionOwner = auctionOwner;
            ImageSize1Id = imageSize1Id;
            ImageSize2Id = imageSize2Id;
            ImageSize3Id = imageSize3Id;
        }
    }
}
