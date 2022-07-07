using Core.Common.Domain;
using System;

namespace UserPayments.Domain.Events
{
    public sealed class PaymentStatusChangedToFailed : Event
    {
        public Guid PaymentId { get; set; }

        public PaymentStatusChangedToFailed() : base("paymentStatusChangedToRejected")
        {
        }
    }
}
