using Core.Common.Domain;

namespace Auctions.Domain.Events
{
    public class AuctionImageReplaced : Event
    {
        public Guid AuctionId { get; }
        public int ImgNum { get; }
        public AuctionImage NewImage { get; }
        public Guid AuctionOwner { get; }

        public AuctionImageReplaced(Guid auctionId, int imgNum, AuctionImage newImage, Guid auctionOwner) : base(EventNames.AuctionImageReplaced)
        {
            AuctionId = auctionId;
            ImgNum = imgNum;
            NewImage = newImage;
            AuctionOwner = auctionOwner;
        }
    }
}
