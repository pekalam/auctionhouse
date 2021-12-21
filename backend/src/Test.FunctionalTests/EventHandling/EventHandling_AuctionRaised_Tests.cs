using System;
using System.Collections.Generic;
using System.Threading;
using Core.Command.Bid;
using Core.Command.Commands.Bid;
using Core.Common.Auth;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusService;
using Core.Query.EventHandlers;
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
            var services = TestDepedencies.Instance.Value;
            var user = User.Create(new Username("testUserName"));
            user.AddCredits(10000);

            services.UserRepository.AddUser(user);

            var userId = UserId.New();
            var product = new Product("test name", "test product description", Condition.New);
            var auctionArgs = new AuctionArgs.Builder()
                    .SetBuyNow(20.0m)
                    .SetName("test auction name")
                    .SetStartDate(DateTime.UtcNow.AddMinutes(10))
                    .SetEndDate(DateTime.UtcNow.AddDays(1))
                    .SetCategory(new Category("test", 0))
                    .SetOwner(userId)
                    .SetProduct(product)
                    .SetTags(new string[]{"tag1", "tag2"})
                    .Build();
            var auction = new Auction(auctionArgs);
            var sem = new SemaphoreSlim(0, 1);


            var eventHandler = new Mock<AuctionRaisedHandler>(services.AppEventBuilder,
                services.DbContext, Mock.Of<IRequestStatusService>(), Mock.Of<ILogger<AuctionRaisedHandler>>());
            eventHandler.CallBase = true;
            eventHandler.Setup(f => f.Consume(It.IsAny<IAppEvent<AuctionRaised>>()))
                .Callback(() => sem.Release());


            var requestStatusService = new Mock<IRequestStatusService>();
            requestStatusService.Setup(service =>
                service.TrySendNotificationToAll("AuctionPriceChanged", It.IsAny<Dictionary<string, object>>()));

            services.SetupEventBus(eventHandler.Object);

            var stubHandler = new BidCommandHandler(services.AuctionRepository,
                services.EventBus, Mock.Of<ILogger<BidCommandHandler>>(), requestStatusService.Object, services.UserRepository);

            services.AuctionRepository.AddAuction(auction);
            var cmd = new BidCommand(auction.AggregateId, 21.0m);
            cmd.SignedInUser = userId;
            stubHandler.Handle(cmd, CancellationToken.None);


            if(!sem.Wait(TimeSpan.FromSeconds(60))){Assert.Fail();};

            eventHandler.Verify(f => f.Consume(It.IsAny<IAppEvent<AuctionRaised>>()), Times.Once);
            requestStatusService.Verify(f => f.TrySendNotificationToAll("AuctionPriceChanged", It.IsAny<Dictionary<string, object>>()), Times.Once());
        }
    }
}