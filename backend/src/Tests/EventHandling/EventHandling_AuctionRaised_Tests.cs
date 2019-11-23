using System;
using System.Threading;
using Command.Bid;
using Core.Command.Bid;
using Core.Common.Auth;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusService;
using Core.Query.Handlers;
using FunctionalTests.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FunctionalTests.EventHandling
{
    public class EventHandling_AuctionRaised_Tests
    {
        [Test]
        public void Test1()
        {


            var user = new UserIdentity() {UserId = Guid.NewGuid(), UserName = "testUserName"};
            var product = new Product("test name", "descripton 111", Condition.New);
            var auctionArgs = new AuctionArgs.Builder()
                    .SetBuyNow(20.0m)
                    .SetStartDate(DateTime.UtcNow.AddMinutes(10))
                    .SetEndDate(DateTime.UtcNow.AddDays(1))
                    .SetCategory(new Category("test", 0))
                    .SetOwner(new UserIdentity(){UserId = Guid.NewGuid(), UserName = "owner"})
                    .SetProduct(product)
                    .SetTags(new string[]{"tag1", "tag2"})
                    .Build();
            var auction = new Auction(auctionArgs);
            var sem = new SemaphoreSlim(0, 1);

            var services = TestDepedencies.Instance.Value;

            var eventHandler = new Mock<AuctionRaisedHandler>(services.AppEventBuilder,
                services.DbContext, Mock.Of<IRequestStatusService>(), Mock.Of<ILogger<AuctionRaisedHandler>>());
            eventHandler.CallBase = true;
            eventHandler.Setup(f => f.Consume(It.IsAny<IAppEvent<AuctionRaised>>()))
                .Callback(() => sem.Release());


            services.SetupEventBus(eventHandler.Object);

            var stubHandler = new BidCommandHandler(services.AuctionRepository,
                services.EventBus, Mock.Of<ILogger<BidCommandHandler>>());

            services.AuctionRepository.AddAuction(auction);
            var cmd = new BidCommand(auction.AggregateId, 21.0m);
            cmd.SignedInUser = user;
            stubHandler.Handle(cmd, CancellationToken.None);


            sem.Wait(TimeSpan.FromSeconds(5));

            eventHandler.Verify(f => f.Consume(It.IsAny<IAppEvent<AuctionRaised>>()), Times.Once);
        }
    }
}