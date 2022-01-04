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
        private AuctionId _auctionId = AuctionId.New();
        private UserId _userId = UserId.New();

        public UnitTest1()
        {
            _userPayments.PendingEvents.Count.Should().Be(1);
        }

        [Fact]
        public void Test1()
        {
            _userPayments.MarkPendingEventsAsHandled();
            var payment = _userPayments.CreateBidPayment(_auctionId, _userId, 10m);

            _userPayments.PendingEvents.First().Should().BeOfType<BidPaymentCreated>();
            _userPayments.PendingEvents.Count.Should().Be(1);
            _userPayments.Payments.Count.Should().Be(1);
            payment.UserId.Should().Be(_userId);
            payment.AuctionId.Should().Be(_auctionId);
            payment.Amount.Should().Be(10m);
            payment.Status.Should().Be(PaymentStatus.InProgress);
            payment.Type.Should().Be(PaymentType.Bid);
            payment.Id.Should().NotBe(Guid.Empty); 
        }

        [Fact]
        public void Test2()
        {
            _userPayments.MarkPendingEventsAsHandled();
            var payment = _userPayments.CreateBuyNowPayment(_auctionId, _userId, 10m);

            _userPayments.PendingEvents.First().Should().BeOfType<BuyNowPaymentCreated>();
            _userPayments.PendingEvents.Count.Should().Be(1);
            _userPayments.Payments.Count.Should().Be(1);
            payment.UserId.Should().Be(_userId);
            payment.AuctionId.Should().Be(_auctionId);
            payment.Amount.Should().Be(10m);
            payment.Status.Should().Be(PaymentStatus.InProgress);
            payment.Type.Should().Be(PaymentType.BuyNow);
            payment.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void Test3()
        {
            _userPayments.CreateBuyNowPayment(_auctionId, _userId, 10m);
            _userPayments.CreateBidPayment(_auctionId, _userId, 10m);

            var events = _userPayments.PendingEvents.ToList();
            _userPayments.MarkPendingEventsAsHandled();
            var recreatedUserPayments = UserPayments.FromEvents(events);

            recreatedUserPayments.Should().BeEquivalentTo(_userPayments);
        }
    }
}
