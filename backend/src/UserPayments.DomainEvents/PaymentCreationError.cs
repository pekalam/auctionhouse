using Core.Common.Domain;

namespace UserPayments.DomainEvents
{
    public static partial class Events
    {
        public static partial class V1
        {
            public class PaymentCreationError : Event
            {
                public PaymentCreationError() : base("paymentCreationError")
                {
                }
            }

            public class BuyNowPaymentFailed : Event
            {
                public BuyNowPaymentFailed() : base("buyNowPaymentFailed")
                {
                }
            }
        }
    }
}