using Core.Common.Domain;

namespace Auctions.DomainEvents
{
    public static partial class Events
    {
        public static partial class V1
        {
            public class AuctionBought : AuctionEvent
            {
                public Guid AuctionId { get; set; }
                public Guid BuyerId { get; set; }
                public string PaymentMethodName { get; set; }
                public decimal Price { get; set; }

                public AuctionBought() : base("auctionBought")
                {
                }
            }

            public class AuctionBuyConfirmed : AuctionEvent
            {
                public Guid AuctionId { get; set; }
                public Guid BuyerId { get; set; }
                public DateTime EndDate { get; set; }

                public AuctionBuyConfirmed() : base("auctionBuyConfirmed")
                {
                }
            }

            public class AuctionBuyConfirmationFailed : AuctionEvent
            {
                public Guid AuctionId { get; set; }

                public AuctionBuyConfirmationFailed() : base("auctionBuyConfirmationFailed")
                {
                }
            }

            public class AuctionBuyCanceled : AuctionEvent
            {
                public Guid AuctionId { get; set; }

                public AuctionBuyCanceled() : base("auctionBuyCanceled")
                {
                }
            }

            public class AuctionBuyCanceledConcurrently : AuctionEvent
            {
                public Guid AuctionId { get; set; }

                public AuctionBuyCanceledConcurrently() : base("auctionBuyCanceled")
                {
                }
            }
        }
    }


}
