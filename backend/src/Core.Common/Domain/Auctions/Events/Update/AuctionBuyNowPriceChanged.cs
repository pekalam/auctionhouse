using System;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events.Update
{
    public class AuctionBuyNowPriceChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public BuyNowPrice BuyNowPrice { get; }
        public UserIdentity Owner { get; }

        public AuctionBuyNowPriceChanged(Guid auctionId, BuyNowPrice buyNowPrice, UserIdentity owner) : base(EventNames.AuctionBuyNowPriceChanged)
        {
            AuctionId = auctionId;
            BuyNowPrice = buyNowPrice;
            Owner = owner;
        }
    }
}
