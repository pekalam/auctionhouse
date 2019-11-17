using System;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionImageAdded : Event
    {
        public AuctionImage AddedImage { get; }
        public int Num { get; }
        public Guid AuctionId { get; }

        public AuctionImageAdded(AuctionImage addedImage, int num, Guid auctionId)
            : base(EventNames.AuctionImageAddedEventName)
        {
            AddedImage = addedImage;
            Num = num;
            AuctionId = auctionId;
        }
    }
}
