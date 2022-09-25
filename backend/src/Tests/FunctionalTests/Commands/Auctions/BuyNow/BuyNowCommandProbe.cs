using System;
using System.Linq;

namespace FunctionalTests.Commands
{
    using Auctions.Domain;
    using Auctions.Domain.Repositories;
    using Common.Application;
    using Core.Common.Domain.Users;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;

    public class BuyNowCommandProbe
    {
        private readonly TestBase _testBase;
        private readonly Guid _auctionId;
        private readonly RequestStatus _status;
        private readonly Type[] _expectedEvents;
        private readonly User _user;
        private readonly BuyNowPrice _buyNowPrice;
        private readonly decimal _initialCredits;


        public BuyNowCommandProbe(TestBase testBase, Guid auctionId, RequestStatus status, Type[] expectedEvents, User user, BuyNowPrice buyNowPrice, decimal initialCredits)
        {
            _testBase = testBase;
            _auctionId = auctionId;
            _status = status;
            _expectedEvents = expectedEvents;
            _user = user;
            _buyNowPrice = buyNowPrice;
            _initialCredits = initialCredits;
        }

        public bool Check()
        {
            var auctions = _testBase.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var auction = auctions.FindAuction(_auctionId);
            var allUserPayments = _testBase.ServiceProvider.GetRequiredService<UserPayments.Domain.Repositories.IUserPaymentsRepository>();
            var users = _testBase.ServiceProvider.GetRequiredService<Users.Domain.Repositories.IUserRepository>();

            var auctionCompleted = auction?.Completed == true;
            var (sagaCompleted, allEventsProcessed) = _testBase.SagaShouldBeCompletedAndAllEventsShouldBeProcessed(_status);
            var expectedEventsAssertion = _testBase.ExpectedEventsShouldBePublished(_expectedEvents);
            var paymentCompletedAssertion = PaymentStatusShouldBe(UserPayments.Domain.PaymentStatus.Completed);
            var userCreditsAssertion = UserCreditsShouldBe(_initialCredits - _buyNowPrice);
            var userReadCreditsAssertion = UserReadCreditsShouldBe(_initialCredits - _buyNowPrice);

            return sagaCompleted &&
            allEventsProcessed &&
            auctionCompleted &&
            expectedEventsAssertion &&
            paymentCompletedAssertion &&
            userCreditsAssertion &&
            userReadCreditsAssertion;


            bool PaymentStatusShouldBe(UserPayments.Domain.PaymentStatus paymentStatus)
            {
                return allUserPayments.WithUserId(new UserPayments.Domain.Shared.UserId(_user.AggregateId.Value)).GetAwaiter().GetResult()
                    ?.Payments.FirstOrDefault(p => p.PaymentTargetId?.Value == _auctionId)?.Status == paymentStatus;
            }

            bool UserReadCreditsShouldBe(decimal credits)
            {
                var userRead = _testBase.ReadModelDbContext.UsersReadModel.Find(u => u.UserIdentity.UserId == _user.AggregateId.ToString()).FirstOrDefault();
                return userRead?.Credits == credits;
            }

            bool UserCreditsShouldBe(decimal credits)
            {
                return users.FindUser(_user.AggregateId)?.Credits == credits;
            }
        }
    }
}
