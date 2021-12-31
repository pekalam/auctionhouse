using Core.Common.Domain;

namespace Auctions.Domain.Events
{
    public static partial class Events
    {
        public static partial class V1
        {
            public class BuyNowTXStarted : Event
            {
                public Guid TransactionId { get; set; }
                public Guid AuctionId { get; set; }
                public Guid BuyerId { get; set; }
                public string PaymentMethod { get; set; }
                public decimal Price { get; set; }

                public BuyNowTXStarted() : base("buyNowTXStarted")
                {
                }
            }

            /// <summary>
            /// Confirmation sent if transaction was commited before lock timeout elapsed
            /// </summary>
            public class BuyNowTXSuccess : Event
            {
                public Guid TransactionId { get; set; }
                public Guid AuctionId { get; set; }
                public Guid BuyerId { get; set; }

                public BuyNowTXSuccess() : base("buyNowTXSuccess")
                {
                }
            }

            /// <summary>
            /// Sent when transaction was commited but it was locked again before commit, after lock timeout elapsed
            /// </summary>
            public class BuyNowTXFailed : Event
            {
                public Guid TransactionId { get; set; }
                public Guid AuctionId { get; set; }

                public BuyNowTXFailed() : base("buyNowTXFailed")
                {
                }
            }
        }
    }


}
