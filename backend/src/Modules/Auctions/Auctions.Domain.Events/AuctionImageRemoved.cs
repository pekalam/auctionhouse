using Core.Common.Domain;

namespace Auctions.DomainEvents
{
    public class AuctionImageRemoved : AuctionEvent
    {
        public Guid AuctionId { get; }
        public int ImgNum { get; }
        public Guid AuctionOwner { get; }

        public AuctionImageRemoved(Guid auctionId, int imgNum, Guid auctionOwner) : base(EventNames.AuctionImageRemoved)
        {
            AuctionId = auctionId;
            ImgNum = imgNum;
            AuctionOwner = auctionOwner;
        }

    }
}
