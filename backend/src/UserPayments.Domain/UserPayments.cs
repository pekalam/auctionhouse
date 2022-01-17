using Core.Common.Domain;
using Core.DomainFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UserPayments.Domain.Events;
using UserPayments.Domain.Shared;

namespace UserPayments.Domain
{
    public class UserPaymentsCreated : Event
    {
        public Guid UserPaymentsId { get; set; }
        public Guid UserId { get; set; }

        public UserPaymentsCreated() : base("userPaymentsCreated")
        {
        }
    }

    public class UserPayments : AggregateRoot<UserPayments, UserPaymentsId>
    {
        private List<Payment> _payments;

        public IReadOnlyList<Payment> Payments => _payments;

        public UserId UserId { get; private set; }

        public UserPayments() { }

        public UserPayments(UserPaymentsId aggregateId, Payment[] payments, UserId userId)
        {
            AggregateId = aggregateId;
            _payments = payments.ToList();
            UserId = userId;
            AddEvent(new UserPaymentsCreated { UserId = userId, UserPaymentsId = aggregateId });
        }

        public static UserPayments CreateNew(UserId userId) => new UserPayments(UserPaymentsId.New(), new Payment[0], userId);

        public Payment CreateBuyNowPayment(TransactionId transactionId, decimal amount, string paymentMethod, PaymentTargetId? paymentTargetId = null)
        {
            var payment = Payment.CreateNew(this, transactionId, UserId, PaymentType.BuyNow, amount, paymentMethod, paymentTargetId);
            _payments.Add(payment);
            return payment;
        }

        public Payment CreateBidPayment(TransactionId transactionId, decimal amount, string paymentMethod) //TODO payment method value object
        {
            var payment = Payment.CreateNew(this, transactionId, UserId, PaymentType.Bid, amount, paymentMethod);
            _payments.Add(payment);
            return payment;
        }

        public void SetPaymentToFailed(PaymentId paymentId) => ChangePaymentStatus(paymentId, PaymentStatus.Failed);
        public void ConfirmPayment(PaymentId paymentId) => ChangePaymentStatus(paymentId, PaymentStatus.Confirmed);
        public void CompletePayment(PaymentId paymentId) => ChangePaymentStatus(paymentId, PaymentStatus.Completed);
        public void SetPaymentFundsRefunded(PaymentId paymentId) => ChangePaymentStatus(paymentId, PaymentStatus.FundsReturned);

        private void ChangePaymentStatus(PaymentId paymentId, PaymentStatus status)
        {
            var payment = _payments.FirstOrDefault(p => p.Id == paymentId.Value);
            if (payment is null)
            {
                throw new PaymentNotFoundException($"Could not find payment with id {paymentId.Value}");
            }

            payment.ChangeStatus(status);
        }

        protected override void Apply(Event @event)
        {
            switch (@event)
            {
                case UserPaymentsCreated e:
                    {
                        AggregateId = new UserPaymentsId(e.UserPaymentsId);
                        UserId = new UserId(e.UserId);
                        _payments = new List<Payment>();
                    }
                    break;
                case BuyNowPaymentCreated e:
                    {
                        var payment = new Payment(this);
                        payment.Apply(e);
                        _payments.Add(payment);
                    }
                    break;
                case BidPaymentCreated e:
                    {
                        var payment = new Payment(this);
                        payment.Apply(e);
                        _payments.Add(payment);
                    }
                    break;
                case PaymentStatusChangedToCompleted e:
                    _payments.First(p => p.Id == e.PaymentId).ApplyInternal(e);
                    break;
                case PaymentStatusChangedToConfirmed e:
                    _payments.First(p => p.Id == e.PaymentId).ApplyInternal(e);
                    break;
                case PaymentStatusChangedToFailed e:
                    _payments.First(p => p.Id == e.PaymentId).ApplyInternal(e);
                    break;
                default:
                    break;
            }
        }
    }

    public sealed class UserPaymentsId : GuidId<UserPaymentsId>
    {
        public UserPaymentsId(Guid value) : base(value)
        {
        }

        public static UserPaymentsId New() => new UserPaymentsId(Guid.NewGuid());
    }

    public class PaymentNotFoundException : DomainException
    {
        public PaymentNotFoundException(string message) : base(message)
        {
        }

        public PaymentNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
