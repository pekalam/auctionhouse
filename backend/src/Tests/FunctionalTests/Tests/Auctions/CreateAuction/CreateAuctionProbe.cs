using Adapter.EfCore.ReadModelNotifications;
using AuctionBids.Domain.Repositories;
using Auctions.Domain.Repositories;
using Common.Application;
using FunctionalTests.Commands;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ReadModel.Contracts.Queries.User.UserAuctions;
using ReadModel.Core.Queries.User.UserAuctions;
using System.Linq;

namespace FunctionalTests.Tests.Auctions.CreateAuction
{
    public class CreateAuctionProbe
    {
        private readonly TestBase _testBase;
        private readonly RequestStatus _requestStatus;

        public CreateAuctionProbe(TestBase testBase, RequestStatus requestStatus)
        {
            _testBase = testBase;
            _requestStatus = requestStatus;
        }

        public bool Check()
        {
            using var scope = _testBase.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var auctions = _testBase.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var auctionBids = _testBase.ServiceProvider.GetRequiredService<IAuctionBidsRepository>();
            var _readModelNotificationsDbContext = scope.ServiceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
            var confirmationsMarkedAsCompleted = _readModelNotificationsDbContext.SagaEventsConfirmations.FirstOrDefault()?.Completed == true;
            var confirmationEventsProcessed = _readModelNotificationsDbContext.SagaEventsToConfirm.All(e => e.Processed);
            var createdAuctionRead = _testBase.SendQuery<UserAuctionsQuery, UserAuctionsQueryResult>(new UserAuctionsQuery()).GetAwaiter().GetResult().Auctions.FirstOrDefault();
            var auctionReadUnlocked = createdAuctionRead != null && !createdAuctionRead.Locked;
            var (sagaCompleted, allEventsProcessed) = _testBase.SagaShouldBeCompletedAndAllEventsShouldBeProcessed(_requestStatus);

            //if (!confirmationsMarkedAsCompleted) outputHelper.WriteLine("Notifications not marked as completed");
            return auctionReadUnlocked && confirmationsMarkedAsCompleted && confirmationEventsProcessed && createdAuctionRead != null && sagaCompleted && allEventsProcessed;
        }
    }
}