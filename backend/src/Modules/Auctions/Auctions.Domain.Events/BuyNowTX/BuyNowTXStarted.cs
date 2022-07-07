using Core.Common.Domain;

namespace Auctions.DomainEvents
{
    public static partial class Events
    {
        public static partial class V1
        {
            public class BuyNowTXStarted : AuctionEvent
            {
                public Guid TransactionId { get; set; }
                public Guid AuctionId { get; set; }
                public Guid BuyerId { get; set; }
                public string PaymentMethodName { get; set; }
                public decimal Price { get; set; }

                public BuyNowTXStarted() : base("buyNowTXStarted")
                {
                }
            }

            /// <summary>
            /// Confirmation sent if transaction was commited before lock timeout elapsed
            /// </summary>
            public class BuyNowTXSuccess : AuctionEvent
            {
                public Guid TransactionId { get; set; }
                public Guid AuctionId { get; set; }
                public Guid BuyerId { get; set; }
                public DateTime EndDate { get; set; }

                public BuyNowTXSuccess() : base("buyNowTXSuccess")
                {
                }
            }

            /// <summary>
            /// Sent when transaction was commited but it was locked again before commit, after lock timeout elapsed
            /// </summary>
            public class BuyNowTXFailed : AuctionEvent
            {
                public Guid TransactionId { get; set; }
                public Guid AuctionId { get; set; }

                public BuyNowTXFailed() : base("buyNowTXFailed")
                {
                }
            }

            public class BuyNowTXCanceled : AuctionEvent
            {
                public Guid TransactionId { get; set; }
                public Guid AuctionId { get; set; }

                public BuyNowTXCanceled() : base("buyNowTXCanceled")
                {
                }
            }

            public class BuyNowTXCanceledConcurrently : AuctionEvent
            {
                public Guid TransactionId { get; set; }
                public Guid AuctionId { get; set; }

                public BuyNowTXCanceledConcurrently() : base("buyNowTXCanceled")
                {
                }
            }
        }
    }


}
