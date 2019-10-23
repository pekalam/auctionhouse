using System;
using System.Collections.Generic;
using System.Threading;
using Core.Command.CreateAuction;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Domain;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Common.SchedulerService;
using Core.Query.Handlers;
using Core.Query.ReadModel;
using FluentAssertions;
using FunctionalTests.EventHandling;
using FunctionalTests.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FunctionalTests.CommandRollback
{
    [TestFixture]
    public class CreateAuctionRollback_test
    {
        public class TestCreateAuctionCommandHandler : CreateAuctionCommandHandler
        {
            private CreateAuctionRollbackHandler _createAuctionRollbackHandler;

            public TestCreateAuctionCommandHandler(IAuctionRepository auctionRepository,
                IUserIdentityService userIdService, IAuctionSchedulerService auctionSchedulerService,
                EventBusService eventBusService, ILogger<CreateAuctionCommandHandler> logger,
                CategoryBuilder categoryBuilder, IUserRepository userRepository, IAuctionCreateSessionService cs,
                CreateAuctionRollbackHandler rollbackHandler) : base(auctionRepository,
                userIdService, auctionSchedulerService, eventBusService, logger, categoryBuilder, userRepository, cs)
            {
                _createAuctionRollbackHandler = rollbackHandler;
            }

            protected override void SetupRollbackHandler()
            {
                RollbackHandlerRegistry.RegisterCommandRollbackHandler(nameof(CreateAuctionCommand),
                    provider => _createAuctionRollbackHandler);
            }
        }

        public class TestCreateAuctionRollbackHandler : CreateAuctionRollbackHandler
        {
            public Action AfterAction { get; set; }

            public TestCreateAuctionRollbackHandler(IImplProvider implProvider) : base(implProvider)
            {
            }

            public override void Rollback(IAppEvent<Event> commandEvent)
            {
                base.Rollback(commandEvent);
                AfterAction?.Invoke();
            }
        }

        public class TestAuctionCreatedHandler : AuctionCreatedHandler
        {
            public TestAuctionCreatedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
                IEventSignalingService eventSignalingService) : base(appEventBuilder, dbContext, eventSignalingService)
            {
            }

            public override void Consume(IAppEvent<AuctionCreated> message)
            {
                throw new Exception();
            }
        }

        [Test]
        public void METHOD()
        {
            var services = TestDepedencies.Instance.Value;
            var user = new User();
            user.Register("testUserName");
            user.MarkPendingEventsAsHandled();
            var product = new Product() {Name = "test product", Description = "desc"};
            var sem = new SemaphoreSlim(0, 1);
            var sem2 = new SemaphoreSlim(0, 1);
            var correlationId = new CorrelationId("test_correlationId");
            var categories = new List<string>()
            {
                "Fake category", "Fake subcategory", "Fake subsubcategory 0"
            };
            var userIdService = new Mock<IUserIdentityService>();
            userIdService.Setup(f => f.GetSignedInUserIdentity())
                .Returns(user.UserIdentity);
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(f => f.FindUser(It.IsAny<UserIdentity>())).Returns(user);

            var command = new CreateAuctionCommand(20.0m, product, DateTime.UtcNow.AddMinutes(10), DateTime.UtcNow.AddDays(12),
                categories, correlationId);

            IAppEvent<AuctionCreated> publishedEvent = null;


            var eventHandler = new Mock<TestAuctionCreatedHandler>(
                services.AppEventBuilder, services.DbContext,
                Mock.Of<IEventSignalingService>());
            eventHandler.CallBase = true;
            eventHandler
                .Setup(f => f.Consume(It.IsAny<IAppEvent<AuctionCreated>>())
                )
                .Callback((IAppEvent<AuctionCreated> ev) =>
                {
                    publishedEvent = ev;
                    sem.Release();
                })
                .CallBase();
            services.SetupEventBus(eventHandler.Object);

            var implProv = new Mock<IImplProvider>();
            implProv.Setup(f => f.Get<IAuctionRepository>()).Returns(services.AuctionRepository);
            RollbackHandlerRegistry.ImplProvider = implProv.Object;
            var testRollbackHandler = new Mock<TestCreateAuctionRollbackHandler>(implProv.Object);
            testRollbackHandler.CallBase = true;
            testRollbackHandler.Setup(f => f.Rollback(It.IsAny<IAppEvent<Event>>())).CallBase();
            testRollbackHandler.Object.AfterAction = () => { sem2.Release(); };


            var session = user.UserIdentity.GetAuctionCreateSession();

            var commandHandler =
                new TestCreateAuctionCommandHandler(services.AuctionRepository, userIdService.Object,
                    services.SchedulerService, services.EventBus,
                    Mock.Of<ILogger<CreateAuctionCommandHandler>>(),
                    new CategoryBuilder(services.CategoryTreeService), userRepository.Object, services.GetAuctionCreateSessionService(session),
                    testRollbackHandler.Object);

            commandHandler.Handle(command, CancellationToken.None);
            sem.Wait(TimeSpan.FromSeconds(5));


            var createdAuciton = services.AuctionRepository.FindAuction(publishedEvent.Event.AuctionId);

            sem2.Wait(TimeSpan.FromSeconds(5));
            var auctionAfterRollback = services.AuctionRepository.FindAuction(publishedEvent.Event.AuctionId);

            testRollbackHandler.Verify(f => f.Rollback(It.IsAny<IAppEvent<Event>>()), Times.Once());
            createdAuciton.Should().NotBe(null);
            auctionAfterRollback.Should().Be(null);
        }
    }
}