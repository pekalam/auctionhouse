using Core.Common.Domain;

namespace UserPayments.Domain.Events
{
    public sealed class PaymentStatusChangedToCompleted : Event
    {
        public Guid PaymentId { get; set; }

        public PaymentStatusChangedToCompleted() : base("paymentStatusChangedToCompleted")
        {
        }
    }
}
