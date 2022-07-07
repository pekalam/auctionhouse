using Auctions.DomainEvents;
using Core.Common.Domain;

namespace Auctions.DomainEvents.Update
{
    public class AuctionBuyNowPriceChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public decimal BuyNowPrice { get; }
        public Guid Owner { get; }

        public AuctionBuyNowPriceChanged(Guid auctionId, decimal buyNowPrice, Guid owner) : base(EventNames.AuctionBuyNowPriceChanged)
        {
            AuctionId = auctionId;
            BuyNowPrice = buyNowPrice;
            Owner = owner;
        }
    }
}
