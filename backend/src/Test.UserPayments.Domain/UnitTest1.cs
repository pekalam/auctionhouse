using System;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace Test.UserPayments_.Domain
{
    using UserPayments.Domain;
    using UserPayments.Domain.Events;
    using UserPayments.Domain.Shared;
    using static UserPayments.DomainEvents.Events.V1;

    [Trait("Category", "Unit")]
    public class UnitTest1
    {
        private TransactionId _transactionId = TransactionId.New();
        private UserId _userId = UserId.New();
        private UserPayments _userPayments;
        private string _paymentMethod = "paymentMethod1";

        public UnitTest1()
        {
            _userPayments = UserPayments.CreateNew(_userId);
            _userPayments.PendingEvents.Count.Should().Be(1);
        }

        [Fact]
        public void Test1()
        {
            _userPayments.MarkPendingEventsAsHandled();
            var payment = _userPayments.CreateBidPayment(_transactionId, 10m, _paymentMethod);

            _userPayments.PendingEvents.First().Should().BeOfType<BidPaymentCreated>();
            _userPayments.PendingEvents.Count.Should().Be(1);
            _userPayments.Payments.Count.Should().Be(1);
            payment.UserId.Should().Be(_userId);
            payment.TransactionId.Should().Be(_transactionId);
            payment.Amount.Should().Be(10m);
            payment.Status.Should().Be(PaymentStatus.InProgress);
            payment.Type.Should().Be(PaymentType.Bid);
            payment.Id.Should().NotBe(Guid.Empty);
            payment.PaymentMethod.Should().Be(_paymentMethod);
        }

        [Fact]
        public void Test2()
        {
            var paymentTargetId = Guid.NewGuid();
            _userPayments.MarkPendingEventsAsHandled();
            var payment = _userPayments.CreateBuyNowPayment(_transactionId, 10m, _paymentMethod, new PaymentTargetId(paymentTargetId));

            _userPayments.PendingEvents.First().Should().BeOfType<BuyNowPaymentCreated>();
            _userPayments.PendingEvents.Count.Should().Be(1);
            _userPayments.Payments.Count.Should().Be(1);
            payment.UserId.Should().Be(_userId);
            payment.TransactionId.Should().Be(_transactionId);
            payment.Amount.Should().Be(10m);
            payment.Status.Should().Be(PaymentStatus.InProgress);
            payment.Type.Should().Be(PaymentType.BuyNow);
            payment.Id.Should().NotBe(Guid.Empty);
            payment.PaymentMethod.Should().Be(_paymentMethod);
            payment.PaymentTargetId.Value.Should().Be(paymentTargetId);
        }

        private void AssertIsRecreated()
        {
            var events = _userPayments.PendingEvents.ToList();
            _userPayments.MarkPendingEventsAsHandled();
            var recreatedUserPayments = UserPayments.FromEvents(events);

            recreatedUserPayments.Should().BeEquivalentTo(_userPayments);
        }

        [Fact]
        public void Test3()
        {
            _userPayments.CreateBuyNowPayment(_transactionId, 10m, _paymentMethod);
            _userPayments.CreateBidPayment(_transactionId, 10m, _paymentMethod);

            AssertIsRecreated();
        }

        [Fact]
        public void Test4()
        {
            var payment = _userPayments.CreateBuyNowPayment(_transactionId, 10m, _paymentMethod);

            _userPayments.ConfirmPayment(payment.Id);
            _userPayments.CompletePayment(payment.Id);

            _userPayments.PendingEvents.Select(e => e.GetType()).Should().BeEquivalentTo(new[] { //TODO helper like this
                typeof(UserPaymentsCreated),
                typeof(BuyNowPaymentCreated),
                typeof(PaymentStatusChangedToConfirmed),
                typeof(BuyNowPaymentConfirmed),
                typeof(PaymentStatusChangedToCompleted),
            });

            AssertIsRecreated();
        }
    }
}
