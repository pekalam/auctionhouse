using Core.Common.Domain;
using System;

namespace UserPayments.Domain.Events
{
    public sealed class PaymentStatusChangedToConfirmed : Event
    {
        public Guid PaymentId { get; set; }

        public PaymentStatusChangedToConfirmed() : base("paymentStatusChangedToAccepted")
        {
        }
    }
}
