using System;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events.Update
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
