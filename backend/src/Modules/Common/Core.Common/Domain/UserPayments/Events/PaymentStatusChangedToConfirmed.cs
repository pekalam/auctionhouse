using System;

namespace Core.Common.Domain.UserPayments
{
    public sealed class PaymentStatusChangedToConfirmed : Event
    {
        public Guid PaymentId { get; set; }

        public PaymentStatusChangedToConfirmed() : base("paymentStatusChangedToAccepted")
        {
        }
    }
}
