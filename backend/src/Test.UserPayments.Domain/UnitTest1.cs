using System;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace Test.UserPayments_.Domain
{
    using UserPayments.Domain;
    using UserPayments.Domain.Events;
    using UserPayments.Domain.Shared;

    public class UnitTest1
    {
        private UserPayments _userPayments = UserPayments.CreateNew(UserId.New());
        private TransactionId _transactionId = TransactionId.New();
        private UserId _userId = UserId.New();

        public UnitTest1()
        {
            _userPayments.PendingEvents.Count.Should().Be(1);
        }

        [Fact]
        public void Test1()
        {
            _userPayments.MarkPendingEventsAsHandled();
            var payment = _userPayments.CreateBidPayment(_transactionId, _userId, 10m);

            _userPayments.PendingEvents.First().Should().BeOfType<BidPaymentCreated>();
            _userPayments.PendingEvents.Count.Should().Be(1);
            _userPayments.Payments.Count.Should().Be(1);
            payment.UserId.Should().Be(_userId);
            payment.TransactionId.Should().Be(_transactionId);
            payment.Amount.Should().Be(10m);
            payment.Status.Should().Be(PaymentStatus.InProgress);
            payment.Type.Should().Be(PaymentType.Bid);
            payment.Id.Should().NotBe(Guid.Empty); 
        }

        [Fact]
        public void Test2()
        {
            _userPayments.MarkPendingEventsAsHandled();
            var payment = _userPayments.CreateBuyNowPayment(_transactionId, _userId, 10m);

            _userPayments.PendingEvents.First().Should().BeOfType<BuyNowPaymentCreated>();
            _userPayments.PendingEvents.Count.Should().Be(1);
            _userPayments.Payments.Count.Should().Be(1);
            payment.UserId.Should().Be(_userId);
            payment.TransactionId.Should().Be(_transactionId);
            payment.Amount.Should().Be(10m);
            payment.Status.Should().Be(PaymentStatus.InProgress);
            payment.Type.Should().Be(PaymentType.BuyNow);
            payment.Id.Should().NotBe(Guid.Empty);
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
            _userPayments.CreateBuyNowPayment(_transactionId, _userId, 10m);
            _userPayments.CreateBidPayment(_transactionId, _userId, 10m);

            AssertIsRecreated();
        }

        [Fact]
        public void Test4()
        {
            var payment = _userPayments.CreateBuyNowPayment(_transactionId, _userId, 10m);

            _userPayments.CompletePayment(payment.Id);

            _userPayments.PendingEvents.Select(e => e.GetType()).Should().BeEquivalentTo(new[] { //TODO helper like this
                typeof(UserPaymentsCreated),
                typeof(BuyNowPaymentCreated),
                typeof(PaymentStatusChangedToCompleted),
            });

            AssertIsRecreated();
        }
    }
}
