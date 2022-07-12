using System.Threading.Tasks;
using UserPayments.Domain.Repositories;
using UserPayments.Domain.Shared;
using UserPayments.Tests.Base;
using Xunit;

namespace Test.Dapper.AuctionhouseDatabase
{
    using Adapter.Dapper.AuctionhouseDatabase.UserPayments_;
    using FluentAssertions;
    using TestConfigurationAccessor;
    using UserPayments.Domain;

    [Trait("Category", "Integration")]
    public class UserPaymentsRepository_Tests
    {
        readonly IUserPaymentsRepository userPaymentsRepo;
        readonly UserId userId = UserId.New();

        public UserPaymentsRepository_Tests()
        {
            var repositorySettings = TestConfig.Instance.GetRepositorySettings();
            userPaymentsRepo = new MsSqlUserPaymentsRepository(repositorySettings);

        }

        [Fact]
        public async Task Can_get_saved_user_payments_by_userid()
        {
            var userPayments = UserPayments.CreateNew(userId);

            userPaymentsRepo.Add(userPayments);
            userPayments.MarkPendingEventsAsHandled();
            var dbPayments = await userPaymentsRepo.WithUserId(userId);

            dbPayments.Should().BeEquivalentTo(userPayments);
        }

        [Fact]
        public async Task Can_get_saved_user_payments_by_id()
        {
            var userPayments = UserPayments.CreateNew(userId);

            userPaymentsRepo.Add(userPayments);
            userPayments.MarkPendingEventsAsHandled();
            var dbPayments = await userPaymentsRepo.WithId(userPayments.AggregateId);

            dbPayments.Should().BeEquivalentTo(userPayments);
        }

        [Fact]
        public async Task Can_update_user_payments()
        {
            var userPayments = UserPayments.CreateNew(userId);
            userPaymentsRepo.Add(userPayments);
            userPayments.MarkPendingEventsAsHandled();

            userPayments.CreateBidPayment(TransactionId.New(), 10m, new GivenPaymentMethod().Build());
            userPaymentsRepo.Update(userPayments);
            userPayments.MarkPendingEventsAsHandled();

            var savedUserPayments = await userPaymentsRepo.WithId(userPayments.AggregateId);
            savedUserPayments.Should().BeEquivalentTo(userPayments);
        }
    }
}
