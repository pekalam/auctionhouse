using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Core.Command;
using Core.Command.CreateAuction;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.SchedulerService;
using FluentAssertions;
using FunctionalTests.EventHandling;
using FunctionalTests.Utils;
using Infrastructure.Services.SchedulerService;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseProviders;
using WireMock.Server;
using WireMock.Settings;

namespace FunctionalTests.Commands
{
    [TestFixture]
    public class CreateAuctionCommand_Tests
    {
        private FluentMockServer server;

        private bool called;
        private DateTime startDate;
        private DateTime endDate;
        private TestCreateAuctionCommandHandler testCommandHandler;
        private TestDepedencies testDepedencies = TestDepedencies.Instance.Value;

        private List<string> categories = new List<string>()
        {
            "Fake category", "Fake subcategory", "Fake subsubcategory 0"
        };

        private void SetUpFakeTimeTaskServer()
        {
            server = FluentMockServer.Start(new FluentMockServerSettings() {Port = 9998});
            var responseProvider = new FakeResponseProvider(HttpStatusCode.OK);
            responseProvider.Callback = message => { called = true; };
            server.Given(Request.Create()
                    .WithPath("/test")
                    .UsingPost())
                .RespondWith(responseProvider);
        }

        private void SetUpCommandHandler()
        {
            var user = new User();
            user.Register("test");
            user.MarkPendingEventsAsHandled();


            var userIdentityService = new Mock<IUserIdentityService>();
            userIdentityService.Setup(service => service.GetSignedInUserIdentity())
                .Returns(user.UserIdentity);

            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(f => f.FindUser(It.IsAny<UserIdentity>()))
                .Returns(user);

            var auctionCreateSessionService = new Mock<IAuctionCreateSessionService>();
            auctionCreateSessionService.Setup(f => f.GetSessionForSignedInUser())
                .Returns(user.UserIdentity.GetAuctionCreateSession());
            auctionCreateSessionService.Setup(f => f.RemoveSessionForSignedInUser());

            var eventBusService = new Mock<EventBusService>(Mock.Of<IEventBus>(), Mock.Of<IAppEventBuilder>());

            testCommandHandler = new TestCreateAuctionCommandHandler(testDepedencies.AuctionRepository,
                userIdentityService.Object,
                new TestAuctionSchedulerService(testDepedencies.TimeTaskClient,
                    testDepedencies.TimetaskServiceSettings),
                eventBusService.Object,
                Mock.Of<ILogger<CreateAuctionCommandHandler>>(),
                new CategoryBuilder(testDepedencies.CategoryTreeService),
                userRepository.Object, auctionCreateSessionService.Object);
        }

        [SetUp]
        public void SetUp()
        {
            SetUpFakeTimeTaskServer();
            called = false;
            startDate = DateTime.UtcNow.AddSeconds(10);
            endDate = DateTime.UtcNow.AddSeconds(20);
            SetUpCommandHandler();
        }

        [TearDown]
        public void TearDown()
        {
            server.Stop();
        }

        [Test]
        public void Handle_when_called_adds_auction_to_repository_and_schedules_end()
        {
            startDate = DateTime.UtcNow.AddSeconds(30);
            endDate = DateTime.UtcNow.AddSeconds(35);
            var command = new CreateAuctionCommand(Decimal.One, new Product {Name = "name", Description = "desc"},
                startDate, endDate,
                categories, new CorrelationId(""));

            testCommandHandler.Handle(command, CancellationToken.None)
                .Wait();

            Thread.Sleep(40_000);

            var added = testDepedencies.AuctionRepository.FindAuction(testCommandHandler.AddedAuction.AggregateId);
            called.Should()
                .BeTrue();
            added.StartDate.Should()
                .Be(startDate);
        }

        [Test]
        public void Handle_when_repository_throws_exception_throws()
        {
            testCommandHandler.AuctionRepositoryThrows = true;
            var command = new CreateAuctionCommand(Decimal.One, new Product {Name = "name", Description = "desc"},
                startDate, endDate, categories, new CorrelationId(""));

            Assert.Throws<Exception>(() =>
            {
                testCommandHandler.Handle(command, CancellationToken.None)
                    .Wait();
            });

            Thread.Sleep(7_000);

            called.Should()
                .BeFalse();
        }

        [Test]
        public void Handle_when_eventbus_throws_exception_throws_and_rolls_back()
        {
            testCommandHandler.EventBusThrows = true;
            var command = new CreateAuctionCommand(Decimal.One, new Product {Name = "name", Description = "desc"},
                startDate,
                endDate, categories, new CorrelationId(""));

            Assert.Throws<Exception>(() =>
            {
                testCommandHandler.Handle(command, CancellationToken.None)
                    .Wait();
            });

            Thread.Sleep(15_000);

            called.Should()
                .BeFalse();
        }
    }

    class FakeResponseProvider : IResponseProvider
    {
        public Action<RequestMessage> Callback { get; set; }
        private HttpStatusCode _responseCode;

        public FakeResponseProvider(HttpStatusCode responseCode)
        {
            _responseCode = responseCode;
        }

        public Task<ResponseMessage> ProvideResponseAsync(RequestMessage requestMessage,
            IFluentMockServerSettings settings)
        {
            Callback?.Invoke(requestMessage);
            return Task.FromResult(new ResponseMessage() {StatusCode = (int) _responseCode});
        }
    }

    class TestCreateAuctionCommandHandler : CreateAuctionCommandHandler
    {
        public bool AuctionRepositoryThrows { get; set; } = false;
        public bool SchedulerServiceThrows { get; set; } = false;
        public bool EventBusThrows { get; set; } = false;

        public Auction AddedAuction { get; private set; }

        public TestCreateAuctionCommandHandler(IAuctionRepository auctionRepository, IUserIdentityService userIdService,
            IAuctionSchedulerService auctionSchedulerService, EventBusService eventBusService,
            ILogger<CreateAuctionCommandHandler> logger, CategoryBuilder categoryBuilder,
            IUserRepository userRepository, IAuctionCreateSessionService auctionCreateSessionService) : base(
            auctionRepository, userIdService, auctionSchedulerService, eventBusService, logger, categoryBuilder,
            userRepository, auctionCreateSessionService)
        {
        }

        protected override void AddToRepository(Auction auction, AtomicSequence<Auction> context)
        {
            if (AuctionRepositoryThrows)
            {
                throw new NotImplementedException();
            }

            base.AddToRepository(auction, context);
            AddedAuction = auction;
        }

        protected override void RollbackAddToRepository(Auction auction, AtomicSequence<Auction> context)
        {
            base.RollbackAddToRepository(auction, context);
        }

        protected override void SheduleAuctionEndTask(Auction auction, AtomicSequence<Auction> context)
        {
            if (SchedulerServiceThrows)
            {
                throw new NotImplementedException();
            }

            base.SheduleAuctionEndTask(auction, context);
        }

        protected override void RollbackScheduleAuctionEndTask(Auction auction, AtomicSequence<Auction> context)
        {
            base.RollbackScheduleAuctionEndTask(auction, context);
        }

        protected override void PublishEvents(Auction auction, User user, CreateAuctionCommand createAuctionCommand)
        {
            if (EventBusThrows)
            {
                throw new NotImplementedException();
            }

            base.PublishEvents(auction, user, createAuctionCommand);
        }
    }

    class TestAuctionSchedulerService : AuctionSchedulerService
    {
        public TestAuctionSchedulerService(ITimeTaskClient timeTaskClient, TimeTaskServiceSettings serviceSettings)
            : base(timeTaskClient, serviceSettings)
        {
        }

        protected override ScheduleRequest<AuctionEndTimeTaskValues> CreateScheduleRequest(Auction auction)
        {
            var request = base.CreateScheduleRequest(auction);
            request.Endpoint = "http://host.docker.internal:9998/test";
            return request;
        }
    }
}