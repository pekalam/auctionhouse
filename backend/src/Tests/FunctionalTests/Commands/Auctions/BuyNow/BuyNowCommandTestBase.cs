using Auctions.Application.Commands.StartAuctionCreateSession;
using Auctions.Domain.Repositories;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using UserPayments.Domain.Repositories;
using Xunit.Abstractions;

namespace FunctionalTests.Commands
{
    using Auctions.Application.Commands.CreateAuction;
    using Auctions.Tests.Base.Domain.Services.Fakes;
    using Core.Common.Domain.Users;
    using ReadModel.Core.Model;
    using ReadModel.Core.Queries.User.UserAuctions;
    using System.Linq;
    using UserPayments.Domain;
    using Users.Domain;
    using Users.Domain.Repositories;
    using Users.Tests.Base.Mocks;

    public class BuyNowCommandTestBase : TestBase, IDisposable
    {
        protected InMemortUserPaymentsRepository allUserPayments;
        protected InMemoryUserRepository users;
        protected ITestOutputHelper outputHelper;


        public BuyNowCommandTestBase(ITestOutputHelper outputHelper)
            : base(outputHelper, "AuctionBids.Application", "Auctions.Application", "UserPayments.Application", "Users.Application", "ReadModel.Core")
        {
            this.outputHelper = outputHelper;
            allUserPayments = (InMemortUserPaymentsRepository)ServiceProvider.GetRequiredService<IUserPaymentsRepository>();
            users = (InMemoryUserRepository)ServiceProvider.GetRequiredService<IUserRepository>();
        }



        protected async Task<AuctionRead> CreateAuction(CreateAuctionCommand cmd)
        {
            //start session
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);
            await Task.Delay(3000);
            //create auction in session
            await SendCommand(cmd);
            await Task.Delay(3000);
            AssertEventual(new CreateAuctionProbe(this).Check);
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
            userPayments.MarkPendingEventsAsHandled();
            allUserPayments.Add(userPayments);
        }
    }
}
