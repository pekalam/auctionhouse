using System;

namespace Core.Common.Domain.UserPayments
{
    public sealed class PaymentStatusChangedToFailed : Event
    {
        public Guid PaymentId { get; set; }

        public PaymentStatusChangedToFailed() : base("paymentStatusChangedToRejected")
        {
        }
    }
}
