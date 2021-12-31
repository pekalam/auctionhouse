using Core.Common.Domain;

namespace Auctions.Domain.Events.Update
{
    public class AuctionBuyNowPriceChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public BuyNowPrice BuyNowPrice { get; }
        public Guid Owner { get; }

        public AuctionBuyNowPriceChanged(Guid auctionId, BuyNowPrice buyNowPrice, Guid owner) : base(EventNames.AuctionBuyNowPriceChanged)
        {
            AuctionId = auctionId;
            BuyNowPrice = buyNowPrice;
            Owner = owner;
        }
    }
}
