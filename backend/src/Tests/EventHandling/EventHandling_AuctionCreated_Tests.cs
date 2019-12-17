using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Core.Command.CreateAuction;
using Core.Common.Auth;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusService;
using Core.Query.EventHandlers;
using Core.Query.ReadModel;
using FluentAssertions;
using FunctionalTests.Utils;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using Tag = Core.Common.Domain.Auctions.Tag;

namespace FunctionalTests.EventHandling
{
    public class TestAuctionCreatedHandler : AuctionCreatedHandler
    {
        public Action<IAppEvent<AuctionCreated>> OnConsumeCalled { get; set; }

        public TestAuctionCreatedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
            IRequestStatusService requestStatusService) : base(appEventBuilder, dbContext, requestStatusService)
        {
        }

        public override void Consume(IAppEvent<AuctionCreated> message)
        {
            base.Consume(message);
            OnConsumeCalled?.Invoke(message);
        }
    }

    public class EventHandling_AuctionCreated_Tests
    {
        private User user;
        private Product product;
        private CreateAuctionCommandHandler commandHandler;

        [SetUp]
        public void SetUp()
        {
            var services = TestDepedencies.Instance.Value;
            user = new User();
            user.Register("testUserName");
            user.MarkPendingEventsAsHandled();
            product = new Product("test product name", "example description", Condition.New);
            services.DbContext.UsersReadModel.InsertOne(new UserRead()
                {UserIdentity = new UserIdentityRead(user.UserIdentity)});
        }

        [TearDown]
        public void TearDown()
        {
            var testDepedencies = TestDepedencies.Instance.Value;
            testDepedencies.AuctionImageRepository.Remove("img1-1");
            testDepedencies.DbContext.UsersReadModel.DeleteMany(FilterDefinition<UserRead>.Empty);
            testDepedencies.DbContext.AuctionsReadModel.DeleteMany(FilterDefinition<AuctionRead>.Empty);
            testDepedencies.DisconnectEventBus();
        }

        private void SetUpCommandHandler()
        {
            var services = TestDepedencies.Instance.Value;

            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(f => f.FindUser(It.IsAny<UserIdentity>()))
                .Returns(user);

            var session = user.UserIdentity.GetAuctionCreateSession();
            session.AddOrReplaceImage(new AuctionImage("img1-1",
                "img1-2", "img1-3"), 0);
            services.AuctionImageRepository.Add("img1-1", new AuctionImageRepresentation()
            {
                Metadata = new AuctionImageMetadata(),
                Img = File.ReadAllBytes("./test_image.jpg")
            });
            var sessionService = services.GetAuctionCreateSessionService(session);

            var handlerDepedencies = new CreateAuctionCommandHandlerDepedencies()
            {
                auctionRepository = services.AuctionRepository,
                auctionSchedulerService = services.SchedulerService,
                eventBusService = services.EventBus,
                logger = Mock.Of<ILogger<CreateAuctionCommandHandler>>(),
                auctionCreateSessionService = sessionService,
                auctionImageRepository = services.AuctionImageRepository,
                categoryBuilder = new CategoryBuilder(services.CategoryTreeService),
                userRepository = userRepository.Object
            };
            commandHandler = new CreateAuctionCommandHandler(handlerDepedencies);
        }

        CreateAuctionCommand GetCreateCommand()
        {
            var categories = new List<string>()
            {
                "Fake category", "Fake subcategory", "Fake subsubcategory 0"
            };
            var cmd = new CreateAuctionCommand(20.0m, product, DateTime.UtcNow.AddMinutes(20),
                DateTime.UtcNow.AddDays(12),
                categories, Tag.From(new[] {"tag1"}), "test name", false);
            cmd.SignedInUser = user.UserIdentity;
            return cmd;
        }

        private bool VerifyEvent(AuctionCreated auctionCreated, CreateAuctionCommand command)
        {
            var v1 = auctionCreated != null;
            var v2 = auctionCreated.AuctionId != Guid.Empty;
            var v3 = auctionCreated.AuctionArgs.Product.Name.Equals(command.Product.Name) &&
                     auctionCreated.AuctionArgs.Product.Description.Equals(command.Product.Description);
            var v4 = auctionCreated.AuctionArgs.BuyNowPrice.Equals(command.BuyNowPrice);
            var v5 = auctionCreated.AuctionArgs.StartDate.Value.CompareTo(command.StartDate) == 0;
            var v6 = auctionCreated.AuctionArgs.Creator.UserId.Equals(user.UserIdentity.UserId);
            return v1 && v2 && v3 && v4 && v5 && v6;
        }

        [Test]
        public void CreateAuctionCommand_handled_and_readmodel_is_created()
        {
            var services = TestDepedencies.Instance.Value;
            var sem = new SemaphoreSlim(0, 1);

            var command = GetCreateCommand();

            string idFromHandler = "";
            CorrelationId correlationIdFromHandler = null;


            var eventHandler = new TestAuctionCreatedHandler(services.AppEventBuilder, services.DbContext, Mock.Of<IRequestStatusService>());
            eventHandler.OnConsumeCalled = ev =>
            {
                Assert.True(VerifyEvent(ev.Event, command));
                idFromHandler = ev.Event.AuctionId.ToString();
                correlationIdFromHandler = ev.CorrelationId;
                sem.Release();
            };

            services.SetupEventBus(eventHandler);
            SetUpCommandHandler();

            //act
            commandHandler.Handle(command, CancellationToken.None);
            if(!sem.Wait(TimeSpan.FromSeconds(60))){Assert.Fail();};

            var auctionReadModel = services.DbContext.AuctionsReadModel.Find(a => a.AuctionId == idFromHandler)
                .FirstOrDefault();
            UserRead userRead = services.DbContext.UsersReadModel
                .Find(f => f.UserIdentity.UserId == user.UserIdentity.UserId.ToString())
                .FirstOrDefault();

            auctionReadModel.Should()
                .NotBeNull();
            auctionReadModel.Product.Should()
                .BeEquivalentTo(product);
            auctionReadModel.Creator.UserId.Should()
                .Be(userRead.UserIdentity.UserId);

            userRead.CreatedAuctions.Count.Should()
                .Be(1);

            services.AuctionImageRepository.Find("img1-1")
                .Metadata.IsAssignedToAuction.Should()
                .BeTrue();

            correlationIdFromHandler.Value.Should().NotBeEmpty();
        }
    }
}