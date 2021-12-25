using Core.Common.Exceptions;
using Core.DomainFramework;
using System;
using System.Diagnostics;

namespace Core.Common.Domain.UserPayments
{
    public sealed class Payment : Entity<PaymentId>
    {
        public AuctionId AuctionId { get; private set; }

        public UserId UserId { get; private set; }

        public PaymentStatus Status { get; private set; }

        public PaymentType Type { get; private set; }

        public decimal Amount { get; private set; }

        public static Payment CreateNew(UserPayments parent, AuctionId auctionId, UserId userId, PaymentType paymentType, decimal amount)
        {
            return new Payment(PaymentId.New(), auctionId, userId, PaymentStatus.InProgress, paymentType, amount, parent);
        }

        /// <summary>
        /// Should be used by aggregate, next step is to apply payment created event with <see cref="ApplyInternal(Event)"/>
        /// </summary>
        internal Payment(IInternalEventAdd parentAdd, IInternalEventApply parentApply = null) : base(parentAdd, parentApply) { }

        public Payment(PaymentId paymentId, AuctionId auctionId, UserId userId, PaymentStatus status, PaymentType paymentType, decimal amount, 
            IInternalEventAdd parentAdd, IInternalEventApply parentApply = null) : base(parentAdd, parentApply)
        {
            //TODO checks
            Id = paymentId;
            AuctionId = auctionId;
            UserId = userId;
            Status = status;
            Type = paymentType;
            Amount = amount;
            if (paymentType == PaymentType.Bid)
            {
                AddEvent(new BidPaymentCreated { AuctionId = auctionId, Amount = amount, UserId = userId, PaymentId = Id });
            }
            else if (paymentType == PaymentType.BuyNow)
            {
                AddEvent(new BuyNowPaymentCreated { AuctionId = auctionId, Amount = amount, UserId = userId, PaymentId = Id });
            }
        }

        public void ChangeStatus(PaymentStatus newStatus)
        {
            if(Status != PaymentStatus.InProgress)
            {
                throw new InvalidPaymentStatusException($"Cannot change status from {PaymentStatus.InProgress} to {newStatus}");
            }
            Status = newStatus;
            switch (newStatus)
            {
                case PaymentStatus.Confirmed:
                    AddEvent(new PaymentStatusChangedToConfirmed { PaymentId = Id });
                    break;
                case PaymentStatus.Failed:
                    AddEvent(new PaymentStatusChangedToFailed { PaymentId = Id });
                    break;
                case PaymentStatus.FundsReturned:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        public override void Apply(Event @event)
        {
            switch (@event)
            {
                case BidPaymentCreated e:
                    Id = new PaymentId(e.PaymentId);
                    AuctionId = new AuctionId(e.AuctionId);
                    UserId = new UserId(e.UserId);
                    Amount = e.Amount;
                    Type = PaymentType.Bid;
                    Status = PaymentStatus.InProgress;
                    break;
                case BuyNowPaymentCreated e:
                    Id = new PaymentId(e.PaymentId);
                    AuctionId = new AuctionId(e.AuctionId);
                    UserId = new UserId(e.UserId);
                    Amount = e.Amount;
                    Type = PaymentType.BuyNow;
                    Status = PaymentStatus.InProgress;
                    break;
                case PaymentStatusChangedToConfirmed e:
                    Debug.Assert(e.PaymentId == Id);
                    Status = PaymentStatus.Confirmed;
                    break;
                case PaymentStatusChangedToFailed e:
                    Debug.Assert(e.PaymentId == Id);
                    Status = PaymentStatus.Failed;
                    break;
                default: throw new NotImplementedException();
            }
        }

        internal new void ApplyInternal(Event @event) => base.ApplyInternal(@event);

    }

    public enum PaymentType
    {
        BuyNow, Bid
    }

    public enum PaymentStatus
    {
        InProgress, Confirmed, Failed, FundsReturned
    }

    public sealed class PaymentId : GuidId<PaymentId>
    {
        public PaymentId(Guid value) : base(value)
        {
        }
        public static PaymentId New() => new PaymentId(Guid.NewGuid());
    }

    public class InvalidPaymentStatusException : DomainException
    {
        public InvalidPaymentStatusException(string message) : base(message)
        {
        }

        public InvalidPaymentStatusException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
