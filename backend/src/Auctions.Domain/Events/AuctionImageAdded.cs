using Core.Common.Domain;

namespace Auctions.Domain.Events
{
    public class AuctionImageAdded : Event
    {
        public AuctionImage AddedImage { get; }
        public int Num { get; }
        public Guid AuctionId { get; }
        public Guid AuctionOwner { get; }

        public AuctionImageAdded(AuctionImage addedImage, int num, Guid auctionId, Guid auctionOwner) : base(EventNames.AuctionImageAddedEventName)
        {
            AddedImage = addedImage;
            Num = num;
            AuctionId = auctionId;
            AuctionOwner = auctionOwner;
        }
    }
}
