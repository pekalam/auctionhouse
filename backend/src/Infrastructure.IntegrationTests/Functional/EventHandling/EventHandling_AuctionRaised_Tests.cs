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
using Core.Common.EventSignalingService;
using Core.Query.Handlers;
using EasyNetQ.AutoSubscribe;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Tests.Functional.EventHandling
{
    public class EventHandling_AuctionRaised_Tests
    {
        [Test]
        public void Test1()
        {
            var stubUser = new UserIdentity() {UserId = Guid.NewGuid(), UserName = "testUserName"};
            var stubProduct = new Product() {Name = "test product", Description = "desc"};
            var stubAuction = new Auction(20.0m, DateTime.UtcNow.AddMinutes(10), DateTime.UtcNow.AddDays(1), stubUser, 
                stubProduct, new Category("test", 0));
            var sem = new SemaphoreSlim(0, 1);

            var services = TestDepedencies.Instance.Value;

            var mockEventHandler = new Mock<AuctionRaisedHandler>(services.AppEventBuilder,
                services.DbContext, Mock.Of<IEventSignalingService>(), Mock.Of<ILogger<AuctionRaisedHandler>>());
            mockEventHandler.CallBase = true;
            mockEventHandler.Setup(f => f.Consume(It.IsAny<IAppEvent<AuctionRaised>>()))
                .Callback(() => sem.Release());


            services.SetupEventBus(mockEventHandler.Object);

            var userIdentityService = new Mock<IUserIdentityService>();
            userIdentityService.Setup(f => f.GetSignedInUserIdentity())
                .Returns(stubUser);

            var stubHandler = new BidCommandHandler(services.AuctionRepository, userIdentityService.Object,
                services.EventBus);

            services.AuctionRepository.AddAuction(stubAuction);
            stubHandler.Handle(new BidCommand(stubAuction.AggregateId, 21.0m, new CorrelationId("123")), CancellationToken.None);


            sem.Wait(TimeSpan.FromSeconds(5));

            mockEventHandler.Verify(f => f.Consume(It.IsAny<IAppEvent<AuctionRaised>>()), Times.Once);
        }
    }
}