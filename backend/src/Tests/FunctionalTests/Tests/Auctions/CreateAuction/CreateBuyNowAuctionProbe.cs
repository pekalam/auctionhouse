using FunctionalTests.Commands;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ReadModel.Core.Queries.User.UserAuctions;
using System.Linq;

namespace FunctionalTests.Tests.Auctions.CreateAuction
{
    public class CreateBuyNowAuctionProbe
    {
        private readonly TestBase _testBase;

        public CreateBuyNowAuctionProbe(TestBase testBase)
        {
            _testBase = testBase;
        }

        public bool Check()
        {
            using var scope = _testBase.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var createdAuctionRead = _testBase.SendQuery<UserAuctionsQuery, UserAuctionsQueryResult>(new UserAuctionsQuery()).GetAwaiter().GetResult().Auctions.FirstOrDefault();
            var auctionReadUnlocked = createdAuctionRead != null && !createdAuctionRead.Locked;

            //if (!confirmationsMarkedAsCompleted) outputHelper.WriteLine("Notifications not marked as completed");
            return auctionReadUnlocked && createdAuctionRead != null;
        }
    }
}