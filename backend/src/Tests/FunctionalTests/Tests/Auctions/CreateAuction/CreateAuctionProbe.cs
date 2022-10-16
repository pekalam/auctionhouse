using Adapter.EfCore.ReadModelNotifications;
using AuctionBids.Domain.Repositories;
using Auctions.Domain.Repositories;
using FunctionalTests.Commands;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ReadModel.Core.Queries.User.UserAuctions;
using System.Linq;

namespace FunctionalTests.Tests.Auctions.CreateAuction
{
    public class CreateAuctionProbe
    {
        private readonly TestBase _testBase;

        public CreateAuctionProbe(TestBase testBase)
        {
            _testBase = testBase;
        }

        public bool Check()
        {
            using var scope = _testBase.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var auctions = _testBase.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var auctionBids = _testBase.ServiceProvider.GetRequiredService<IAuctionBidsRepository>();
            var _readModelNotificationsDbContext = scope.ServiceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
            var confirmationsMarkedAsCompleted = _readModelNotificationsDbContext.SagaEventsConfirmations.FirstOrDefault()?.Completed == true;
            var confirmationEventsProcessed = _readModelNotificationsDbContext.SagaEventsToConfirm.All(e => e.Processed);
            var createdAuction = _testBase.SendQuery<UserAuctionsQuery, UserAuctionsQueryResult>(new UserAuctionsQuery()).GetAwaiter().GetResult().Auctions.FirstOrDefault();
            var auctionUnlocked = createdAuction != null && !createdAuction.Locked;

            //if (!confirmationsMarkedAsCompleted) outputHelper.WriteLine("Notifications not marked as completed");
            return auctionUnlocked && confirmationsMarkedAsCompleted && confirmationEventsProcessed && createdAuction != null;
        }
    }
}