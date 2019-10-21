using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Command.CreateAuction;
using Core.Common.Auth;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Query.Handlers;
using Core.Query.ReadModel;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace Infrastructure.Tests.Functional.EventHandling
{
    public class EventHandling_AuctionCreated_Tests
    {
        private bool VerifyEvent(AuctionCreated auctionCreated, CreateAuctionCommand command)
        {
            var v1 = auctionCreated != null;
            var v2 = auctionCreated.AuctionId != Guid.Empty;
            var v3 = auctionCreated.Product.Name.Equals(command.Product.Name) &&
                     auctionCreated.Product.Description.Equals(command.Product.Description);
            var v4 = auctionCreated.BuyNowPrice.Equals(command.BuyNowPrice);
            var v5 = auctionCreated.StartDate.CompareTo(command.StartDate) == 0;
            return v1 && v2 && v3 && v4 && v5;
        }

        [Test]
        public void Test1()
        {
            //arrange
            var user = new User();
            user.Register("testUserName");
            user.MarkPendingEventsAsHandled();
            var product = new Product() {Name = "test product", Description = "desc"};
            var sem = new SemaphoreSlim(0, 1);
            var correlationId = new CorrelationId("test_correlationId");
            var categories = new List<string>()
            {
                "Fake category", "Fake subcategory", "Fake subsubcategory 0"
            };
            var command = new CreateAuctionCommand(20.0m, product, DateTime.UtcNow.AddMinutes(20), DateTime.UtcNow.AddDays(12),
                categories, correlationId);
            string idFromHandler = "";
            CorrelationId correlationIdFromHandler = null;
            var services = TestDepedencies.Instance.Value;

            var eventHandler = new Mock<AuctionCreatedHandler>(
                services.AppEventBuilder, services.DbContext,
                Mock.Of<IEventSignalingService>());
            eventHandler.CallBase = true;
            eventHandler
                .Setup(f => f.Consume(
                    It.Is<IAppEvent<AuctionCreated>>(v => VerifyEvent(v.Event, command)))
                )
                .Callback((IAppEvent<AuctionCreated> ev) =>
                {
                    idFromHandler = ev.Event.AuctionId.ToString();
                    correlationIdFromHandler = ev.CorrelationId;
                    sem.Release();
                })
                .CallBase();

            services.SetupEventBus(eventHandler.Object);

            var userIdService = new Mock<IUserIdentityService>();
            userIdService.Setup(f => f.GetSignedInUserIdentity())
                .Returns(user.UserIdentity);

            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(f => f.FindUser(It.IsAny<UserIdentity>())).Returns(user);

            var session = user.UserIdentity.GetAuctionCreateSession();
            var commandHandler =
                new CreateAuctionCommandHandler(services.AuctionRepository, userIdService.Object,
                    services.SchedulerService, services.EventBus,
                    Mock.Of<ILogger<CreateAuctionCommandHandler>>(),
                    new CategoryBuilder(services.CategoryTreeService), userRepository.Object,
                    services.GetAuctionCreateSessionService(session));

            services.DbContext.UsersReadModel.InsertOne(new UserReadModel() {UserIdentity = new UserIdentityReadModel(user.UserIdentity) });

            //act
            commandHandler.Handle(command, CancellationToken.None);
            sem.Wait(TimeSpan.FromSeconds(5));
            Thread.Sleep(4000);
            var auctionReadModel = services.DbContext.AuctionsReadModel.Find(a => a.AuctionId == idFromHandler)
                .FirstOrDefault();
            UserReadModel userReadModel = services.DbContext.UsersReadModel
                .Find(f => f.UserIdentity.UserId == user.UserIdentity.UserId.ToString()).FirstOrDefault();


            //assert
            eventHandler.Verify(f => f.Consume(It.Is<IAppEvent<AuctionCreated>>(v => VerifyEvent(v.Event, command))),
                Times.Once);

            auctionReadModel.Should().NotBeNull();
            auctionReadModel.Product.Should().BeEquivalentTo(product);
            auctionReadModel.Creator.Should().BeEquivalentTo(userReadModel.UserIdentity);

            userReadModel.CreatedAuctions.Count.Should().Be(1);

            correlationId.Value.Should().BeEquivalentTo(correlationIdFromHandler.Value);
        }
    }
}