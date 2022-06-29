using Core.Common.Domain;

namespace Auctions.DomainEvents
{
    public class AuctionImageAdded : AuctionEvent
    {
        public string AddedImageSize1Id { get; }
        public string AddedImageSize2Id { get; }
        public string AddedImageSize3Id { get; }
        public int Num { get; }
        public Guid AuctionId { get; }
        public Guid AuctionOwner { get; }

        public AuctionImageAdded(int num, Guid auctionId, Guid auctionOwner, string addedImageSize1Id, string addedImageSize2Id, string addedImageSize3Id) : base(EventNames.AuctionImageAddedEventName)
        {
            Num = num;
            AuctionId = auctionId;
            AuctionOwner = auctionOwner;
            AddedImageSize1Id = addedImageSize1Id;
            AddedImageSize2Id = addedImageSize2Id;
            AddedImageSize3Id = addedImageSize3Id;
        }
    }
}
