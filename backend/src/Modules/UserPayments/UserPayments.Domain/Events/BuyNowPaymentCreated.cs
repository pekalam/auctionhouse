using Core.Common.Domain;

namespace UserPayments.Domain.Events
{
    public sealed class BuyNowPaymentCreated : Event
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public Guid TransactionId { get; set; }
        public Guid? PaymentTargetId { get; set; }

        public decimal Amount { get; set; }
        public string PaymentMethodName { get; set; }

        public BuyNowPaymentCreated() : base("buyNowPaymentCreated")
        {
        }
    }
}
