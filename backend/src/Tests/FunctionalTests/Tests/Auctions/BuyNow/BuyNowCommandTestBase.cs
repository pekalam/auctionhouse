using Auctions.Application.Commands.StartAuctionCreateSession;
using Auctions.Domain.Repositories;
using FunctionalTests.Commands;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using UserPayments.Domain.Repositories;
using Xunit.Abstractions;

namespace FunctionalTests.Tests.Auctions.BuyNow
{
    using Core.Common.Domain.Users;
    using FunctionalTests.Tests.Auctions.CreateAuction;
    using global::Auctions.Application.Commands.CreateAuction;
    using global::Users.Domain.Repositories;
    using ReadModel.Contracts.Model;
    using ReadModel.Contracts.Queries.User.UserAuctions;
    using ReadModel.Core.Model;
    using ReadModel.Core.Queries.User.UserAuctions;
    using System.Linq;
    using UserPayments.Domain;

    public class BuyNowCommandTestBase : TestBase, IDisposable
    {
        protected IUserPaymentsRepository allUserPayments;
        protected IUserRepository users;
        protected ITestOutputHelper outputHelper;


        public BuyNowCommandTestBase(ITestOutputHelper outputHelper)
            : base(outputHelper, "AuctionBids.Application", "Auctions.Application", "UserPayments.Application", "Users.Application", "ReadModel.Core", "ReadModel.Contracts")
        {
            this.outputHelper = outputHelper;
            allUserPayments = ServiceProvider.GetRequiredService<IUserPaymentsRepository>();
            users = ServiceProvider.GetRequiredService<IUserRepository>();
        }



        protected async Task<AuctionRead> CreateAuction(CreateAuctionCommand cmd)
        {
            //start session
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);
            await Task.Delay(3000);
            //create auction in session
            var createRequestStatus = await SendCommand(cmd);
            await Task.Delay(3000);
            AssertEventual(new CreateAuctionProbe(this, createRequestStatus).Check);
            InMemoryEventBusDecorator.ClearSentEvents();

            var createdAuctions = await SendQuery<UserAuctionsQuery, UserAuctionsQueryResult>(new UserAuctionsQuery());
            return createdAuctions.Auctions.First();
        }

        protected async Task<User> CreateUser(decimal initialCredits)
        {
            ChangeSignedInUser(initialCredits);
            return SignedInUser;
        }

        protected void CreateUserPayments(User user)
        {
            var userPayments = UserPayments.CreateNew(new global::UserPayments.Domain.Shared.UserId(user.AggregateId.Value));
            allUserPayments.Add(userPayments);
        }
    }
}
