using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Domain.Repositories;
using UserPayments.Domain.Shared;
using Xunit;

namespace Test.Dapper.AuctionhouseDatabase
{
    using Adapter.Dapper.AuctionhouseDatabase;
    using Adapter.Dapper.AuctionhouseDatabase.UserPayments_;
    using FluentAssertions;
    using UserPayments.Domain;

    [Trait("Category", "Integration")]
    public class UserPaymentsRepository_Tests
    {
        IUserPaymentsRepository userPaymentsRepo;
        UserId userId = UserId.New();

        public UserPaymentsRepository_Tests()
        {
            var serverOpt = new MsSqlConnectionSettings()
            {
                //ConnectionString = TestContextUtils.GetParameterOrDefault("sqlserver",
                //"Data Source=.;Initial Catalog=AuctionhouseDatabase;Integrated Security=False;User ID=sa;PWD=Qwerty1234;")
                ConnectionString = "Server=127.0.0.1;Database=AuctionhouseDatabase;TrustServerCertificate=True;User ID=sa;Password=Qwerty1234;"
            };
            userPaymentsRepo = new MsSqlUserPaymentsRepository(serverOpt);

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

            userPayments.CreateBidPayment(TransactionId.New(), 10m);
            userPaymentsRepo.Update(userPayments);
            userPayments.MarkPendingEventsAsHandled();

            var savedUserPayments = await userPaymentsRepo.WithId(userPayments.AggregateId);
            savedUserPayments.Should().BeEquivalentTo(userPayments);
        }
    }
}
