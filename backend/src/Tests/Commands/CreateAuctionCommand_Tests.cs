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
        private User user;

        public CreateAuctionCommand_Tests()
        {
            SetUpFakeTimeTaskServer();
        }

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
            user = new User();
            user.Register("test");
            user.MarkPendingEventsAsHandled();

            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(f => f.FindUser(It.IsAny<UserIdentity>()))
                .Returns(user);

            var auctionCreateSessionService = new Mock<IAuctionCreateSessionService>();
            auctionCreateSessionService.Setup(f => f.GetExistingSession())
                .Returns(user.UserIdentity.GetAuctionCreateSession());
            auctionCreateSessionService.Setup(f => f.RemoveSession());

            var eventBusService = new Mock<EventBusService>(Mock.Of<IEventBus>(), Mock.Of<IAppEventBuilder>());

            var handlerDepedencies = new CreateAuctionCommandHandlerDepedencies()
            {
                auctionRepository = testDepedencies.AuctionRepository,
                auctionSchedulerService = new TestAuctionSchedulerService(testDepedencies.TimeTaskClient,
                    testDepedencies.TimetaskServiceSettings),
                eventBusService = eventBusService.Object,
                logger = Mock.Of<ILogger<CreateAuctionCommandHandler>>(),
                auctionCreateSessionService = auctionCreateSessionService.Object,
                auctionImageRepository = testDepedencies.AuctionImageRepository,
                categoryBuilder = new CategoryBuilder(testDepedencies.CategoryTreeService),
                userRepository = userRepository.Object
            };
            testCommandHandler = new TestCreateAuctionCommandHandler(handlerDepedencies);
        }

        [SetUp]
        public void SetUp()
        {
            AuctionConstantsFactory.MinAuctionTimeM = -1;
            called = false;
            startDate = DateTime.UtcNow.AddSeconds(10);
            endDate = DateTime.UtcNow.AddSeconds(20);
            SetUpCommandHandler();
        }


        [Test]
        public void Handle_when_called_adds_auction_to_repository_and_schedules_end()
        {
            startDate = DateTime.UtcNow;
            endDate = DateTime.UtcNow.AddSeconds(35);
            var command = new CreateAuctionCommand(Decimal.One,
                new Product("name", "desc", Condition.New),
                startDate, endDate,
                categories, Tag.From(new []{"tag1"}), "test name", false);
            command.SignedInUser = user.UserIdentity;

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
            var command = new CreateAuctionCommand(Decimal.One,
                new Product("name", "desc", Condition.New),
                startDate, endDate, categories, Tag.From(new []{"tag1"}), "test name", false);
            command.SignedInUser = user.UserIdentity;


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
            var command = new CreateAuctionCommand(Decimal.One,
                new Product("name", "desc", Condition.New),
                startDate,
                endDate, categories, Tag.From(new []{"tag1"}), "test name", false);
            command.SignedInUser = user.UserIdentity;


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

        public TestCreateAuctionCommandHandler(CreateAuctionCommandHandlerDepedencies depedencies) : base(depedencies)
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

        protected override void AddToRepository_Rollback(Auction auction, AtomicSequence<Auction> context)
        {
            base.AddToRepository_Rollback(auction, context);
        }

        protected override void SheduleAuctionEndTask(Auction auction, AtomicSequence<Auction> context)
        {
            if (SchedulerServiceThrows)
            {
                throw new NotImplementedException();
            }

            base.SheduleAuctionEndTask(auction, context);
        }

        protected override void ScheduleAuctionEndTask_Rollback(Auction auction, AtomicSequence<Auction> context)
        {
            base.ScheduleAuctionEndTask_Rollback(auction, context);
        }

        protected override void PublishEvents(Auction auction, User user, CreateAuctionCommand createAuctionCommand, CorrelationId correlationId)
        {
            if (EventBusThrows)
            {
                throw new NotImplementedException();
            }

            base.PublishEvents(auction, user, createAuctionCommand, correlationId);
        }
    }

    class TestAuctionSchedulerService : AuctionSchedulerService
    {
        private readonly TimeTaskServiceSettings _serviceSettings;

        public TestAuctionSchedulerService(ITimeTaskClient timeTaskClient, TimeTaskServiceSettings serviceSettings)
            : base(timeTaskClient, serviceSettings)
        {
            _serviceSettings = serviceSettings;
        }

        protected override ScheduleRequest<AuctionEndTimeTaskValues> CreateScheduleRequest(Auction auction)
        {
            var request = base.CreateScheduleRequest(auction);
            request.Endpoint = _serviceSettings.AuctionEndEchoTaskEndpoint;
            return request;
        }


    }
}