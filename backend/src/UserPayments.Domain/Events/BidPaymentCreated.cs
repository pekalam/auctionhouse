using Core.Common.Domain;
using System;

namespace UserPayments.Domain.Events
{
    public sealed class BidPaymentCreated : Event
    {
        public Guid PaymentId { get; set; }

        public Guid AuctionId { get; set; }

        public Guid UserId { get; set; }

        public decimal Amount { get; set; }

        public BidPaymentCreated() : base("bidPaymentCreated")
        {
        }
    }
}
