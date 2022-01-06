using Core.Common.Domain;

namespace UserPayments.DomainEvents
{
    public static partial class Events
    {
        public static partial class V1
        {
            public class BuyNowPaymentConfirmed : Event
            {
                public Guid TransactionId { get; set; }

                public BuyNowPaymentConfirmed() : base("buyNowPaymentConfirmed")
                {
                }
            }
        }
    }


}