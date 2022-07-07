﻿using Core.Common.Domain;
using Core.DomainFramework;
using System;
using System.Diagnostics;
using UserPayments.Domain.Events;
using UserPayments.Domain.Shared;
using static UserPayments.DomainEvents.Events.V1;

namespace UserPayments.Domain
{
    /// <summary>
    /// TransactionId generated by other module, representing action that led to creation of <see cref="Payment"/>
    /// </summary>
    public class TransactionId : GuidId<TransactionId>
    {
        public TransactionId(Guid value) : base(value)
        {
        }

        public static TransactionId New() => new TransactionId(Guid.NewGuid());
    }

    /// <summary>
    /// If payment is connected with some object from another module (auction, credits etc..) then this class represents unique id
    /// of this object
    /// </summary>
    public class PaymentTargetId : GuidId<PaymentTargetId>
    {
        public PaymentTargetId(Guid value) : base(value)
        {
        }
    }

    public sealed class Payment : Entity<PaymentId>
    {
        public UserId UserId { get; private set; }

        public TransactionId TransactionId { get; set; }

        public PaymentTargetId? PaymentTargetId { get; set; }

        public PaymentStatus Status { get; private set; }

        public PaymentType Type { get; private set; }

        public decimal Amount { get; private set; }

        public PaymentMethod PaymentMethod { get; private set; }

        internal static Payment CreateNew(UserPayments parent, TransactionId transactionId, UserId userId, PaymentType paymentType, decimal amount, PaymentMethod paymentMethod, PaymentTargetId? paymentTargetId = null)
        {
            return new Payment(PaymentId.New(), transactionId, userId, PaymentStatus.InProgress, paymentType, amount, paymentMethod, parent, paymentTargetId);
        }

        /// <summary>
        /// Should be used by aggregate, next step is to apply payment created event with <see cref="ApplyInternal(Event)"/>
        /// </summary>
        internal Payment(IInternalEventAdd parentAdd, IInternalEventApply parentApply = null) : base(parentAdd, parentApply) { }

        internal Payment(PaymentId paymentId, TransactionId transactionId, UserId userId, PaymentStatus status, PaymentType paymentType, decimal amount,
            PaymentMethod paymentMethod, IInternalEventAdd parentAdd, PaymentTargetId? paymentTargetId = null, IInternalEventApply? parentApply = null) : base(parentAdd, parentApply)
        {
            //TODO checks
            PaymentTargetId = paymentTargetId;
            TransactionId = transactionId;
            Id = paymentId;
            UserId = userId;
            Status = status;
            Type = paymentType;
            Amount = amount;
            PaymentMethod = paymentMethod;
            if (paymentType == PaymentType.Bid)
            {
                AddEvent(new BidPaymentCreated { TransactionId = transactionId, Amount = amount, UserId = userId, PaymentId = Id, PaymentMethodName = paymentMethod.Name, PaymentTargetId = (paymentTargetId != null ? (Guid)paymentTargetId : null)  });
            }
            else if (paymentType == PaymentType.BuyNow)
            {
                AddEvent(new BuyNowPaymentCreated { TransactionId = transactionId, Amount = amount, UserId = userId, PaymentId = Id, PaymentMethodName = paymentMethod.Name, PaymentTargetId = (paymentTargetId != null ? (Guid)paymentTargetId : null) });
            }
        }

        public void ChangeStatus(PaymentStatus newStatus)
        {
            switch (newStatus)
            {
                case PaymentStatus.Confirmed:
                    if (Status != PaymentStatus.InProgress)
                    {
                        throw new InvalidPaymentStatusException($"Cannot change status from {Status} to {newStatus}");
                    }
                    if (Type == PaymentType.BuyNow)
                    {
                        AddEvent(new BuyNowPaymentConfirmed() { TransactionId = TransactionId });
                    }
                    AddEvent(new PaymentStatusChangedToConfirmed { PaymentId = Id });
                    break;
                case PaymentStatus.Completed:
                    if (Status != PaymentStatus.Confirmed)
                    {
                        throw new InvalidPaymentStatusException($"Cannot change status from {Status} to {newStatus}");
                    }
                    AddEvent(new PaymentStatusChangedToCompleted { PaymentId = Id });
                    break;
                case PaymentStatus.Failed:
                    if (Status != PaymentStatus.InProgress)
                    {
                        throw new InvalidPaymentStatusException($"Cannot change status from {Status} to {newStatus}");
                    }
                    if (Type == PaymentType.BuyNow)
                    {
                        AddEvent(new BuyNowPaymentFailed() { TransactionId = TransactionId });
                    }
                    AddEvent(new PaymentStatusChangedToFailed { PaymentId = Id });
                    break;
                case PaymentStatus.FundsReturned:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
            Status = newStatus;
        }

        public override void Apply(Event @event)
        {
            switch (@event)
            {
                case BidPaymentCreated e:
                    Id = new PaymentId(e.PaymentId);
                    TransactionId = new TransactionId(e.TransactionId);
                    UserId = new UserId(e.UserId);
                    Amount = e.Amount;
                    Type = PaymentType.Bid;
                    Status = PaymentStatus.InProgress;
                    PaymentTargetId = e.PaymentTargetId.HasValue ? new PaymentTargetId(e.PaymentTargetId.Value) : null;
                    PaymentMethod = new(e.PaymentMethodName);
                    break;
                case BuyNowPaymentCreated e:
                    Id = new PaymentId(e.PaymentId);
                    TransactionId = new TransactionId(e.TransactionId);
                    UserId = new UserId(e.UserId);
                    Amount = e.Amount;
                    Type = PaymentType.BuyNow;
                    Status = PaymentStatus.InProgress;
                    PaymentTargetId = e.PaymentTargetId.HasValue ? new PaymentTargetId(e.PaymentTargetId.Value) : null;
                    PaymentMethod = new(e.PaymentMethodName);
                    break;
                case PaymentStatusChangedToConfirmed e:
                    Debug.Assert(e.PaymentId == Id);
                    Status = PaymentStatus.Confirmed;
                    break;
                case PaymentStatusChangedToCompleted e:
                    Debug.Assert(e.PaymentId == Id);
                    Status = PaymentStatus.Completed;
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
        InProgress, Confirmed, Failed, FundsReturned, Completed
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