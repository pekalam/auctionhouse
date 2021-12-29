using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusSender;
using Infrastructure.Services.EventBus;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace IntegrationTests
{
    public class TestQueuedCommand : ICommand
    {

    }

    public class TestQueuedCommandHandler : CommandHandlerBase<TestQueuedCommand>
    {
        public TestQueuedCommandHandler(ILogger logger) : base(logger)
        {
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<TestQueuedCommand> request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    [TestFixture]
    public class RabbitMqQueuedCommandBus_QueuedCommandHandling_Tests
    {
        [Test]
        public void Calls_queued_ws_command_handler_for_sent_cmd()
        {
            var signedInUser = GivenUserId();
            var cmd = GivenTestCommand();
            var appCmd = GivenTestAppCommand(signedInUser, cmd, false);
            var queuedCmd = new QueuedCommand()
            {
                AppCommand = appCmd,
            };
            var sem = new SemaphoreSlim(0, 1);

            var mockUserIdentityService = MockUserIdentityService(signedInUser);
            var mockMediatr = MockMediatR(queuedCmd);
            var mockRequestStatusService = new Mock<IRequestStatusSender>();

            var mockWSQueuedCommandHandler = new Mock<WSQueuedCommandHandler>(mockRequestStatusService.Object, mockMediatr.Object, Mock.Of<ILogger<WSQueuedCommandHandler>>());
            mockWSQueuedCommandHandler.Setup(handler => handler.Handle(It.IsAny<QueuedCommand>()))
                .Callback(() => sem.Release())
                .CallBase();
            var mockImplProvider = MockWSImplProvider(mockUserIdentityService, mockWSQueuedCommandHandler);

            RabbitMqEventBusTestUtils.QueuedCommandBus.Value.CancelCommandSubscriptions();

            RabbitMqEventBusTestUtils.QueuedCommandBus.Value.PreparePublish(mockImplProvider.Object, cmd);
            RabbitMqEventBusTestUtils.QueuedCommandBus.Value.Publish<TestQueuedCommand>(queuedCmd);

            if (!sem.Wait(TimeSpan.FromSeconds(60))) { Assert.Fail(); };

            mockRequestStatusService.Verify(service => service.TrySendRequestCompletionToUser(
                appCmd.CommandContext.Name, It.Is<CommandId>(c => c.Id == appCmd.CommandContext.CommandId.Id), appCmd.CommandContext.User, null
            ), Times.Once());
        }

        private static Mock<IImplProvider> MockWSImplProvider(Mock<IUserIdentityService> mockUserIdentityService, Mock<WSQueuedCommandHandler> testQueuedCommandHandler)
        {
            var mockImplProvider = new Mock<IImplProvider>();
            mockImplProvider.Setup(provider => provider.Get<IUserIdentityService>()).Returns(mockUserIdentityService.Object);
            mockImplProvider.Setup(provider => provider.Get<WSQueuedCommandHandler>()).Returns(testQueuedCommandHandler.Object);
            return mockImplProvider;
        }

        [Test]
        public void Calls_queued_http_command_handler_for_sent_cmd()
        {
            var signedInUser = GivenUserId();
            var cmd = GivenTestCommand();
            var appCmd = GivenTestAppCommand(signedInUser, cmd, true);
            var queuedCmd = new QueuedCommand()
            {
                AppCommand = appCmd,
            };
            var sem = new SemaphoreSlim(0, 2);

            var mockUserIdentityService = MockUserIdentityService(signedInUser);
            var mockQueuedCommandStatusStorage = MockQueuedCommandStatusStorage(sem);
            var mockMediatr = MockMediatR(queuedCmd);
            Mock<HTTPQueuedCommandHandler> mockQueuedCommandHandler = MockQueuedCommandHandler(sem, mockQueuedCommandStatusStorage, mockMediatr);
            var mockImplProvider = MockImplProvider(mockUserIdentityService, mockQueuedCommandHandler);

            RabbitMqEventBusTestUtils.QueuedCommandBus.Value.CancelCommandSubscriptions();

            RabbitMqEventBusTestUtils.QueuedCommandBus.Value.PreparePublish(mockImplProvider.Object, cmd);
            RabbitMqEventBusTestUtils.QueuedCommandBus.Value.Publish<TestQueuedCommand>(queuedCmd);


            if (!sem.Wait(TimeSpan.FromSeconds(60)))
            {
                Assert.Fail();
            }
            if (!sem.Wait(TimeSpan.FromSeconds(60)))
            {
                Assert.Fail();
            }
            mockQueuedCommandStatusStorage
                .Verify(storage => storage.UpdateCommandStatus(It.IsAny<RequestStatus>(), It.IsAny<CommandId>()), Times.Once());
            mockMediatr.Verify(m => m.Send(It.IsAny<IRequest<RequestStatus>>(), It.IsAny<CancellationToken>()), Times.Once());
            mockQueuedCommandHandler.Verify(m => m.Handle(It.IsAny<QueuedCommand>()), Times.Once());
            Assert.AreEqual(0, sem.CurrentCount);
        }

        private static AppCommand<TestQueuedCommand> GivenTestAppCommand(UserId signedInUser, TestQueuedCommand cmd, bool http)
        {
            return new AppCommand<TestQueuedCommand> { Command = cmd, CommandContext = 
                http ?
                CommandContext.CreateHttpQueued(signedInUser, nameof(TestQueuedCommand)) :
                CommandContext.CreateWSQueued(signedInUser, nameof(TestQueuedCommand))};
        }

        private static TestQueuedCommand GivenTestCommand()
        {
            return new TestQueuedCommand();
        }

        private static UserId GivenUserId()
        {
            return UserId.New();
        }

        private static Mock<HTTPQueuedCommandHandler> MockQueuedCommandHandler(SemaphoreSlim sem, Mock<IHTTPQueuedCommandStatusStorage> mockQueuedCommandStatusStorage, Mock<IMediator> mediatrMock)
        {
            var testQueuedCommandHandler = new Mock<HTTPQueuedCommandHandler>(
                            mockQueuedCommandStatusStorage.Object,
                            mediatrMock.Object,
                            Mock.Of<ILogger<HTTPQueuedCommandHandler>>()
                        );
            testQueuedCommandHandler.Setup(handler => handler.Handle(It.IsAny<QueuedCommand>()))
                .Callback(() => sem.Release())
                .CallBase();
            return testQueuedCommandHandler;
        }

        private static Mock<IImplProvider> MockImplProvider(Mock<IUserIdentityService> mockUserIdentityService, Mock<HTTPQueuedCommandHandler> testQueuedCommandHandler)
        {
            var mockImplProvider = new Mock<IImplProvider>();
            mockImplProvider.Setup(provider => provider.Get<IUserIdentityService>())
                .Returns(mockUserIdentityService.Object);
            mockImplProvider.Setup(provider => provider.Get<HTTPQueuedCommandHandler>())
                .Returns(testQueuedCommandHandler.Object);
            return mockImplProvider;
        }

        private static Mock<IMediator> MockMediatR(QueuedCommand queuedCmd)
        {
            // simulates handler that returns completed status
            var mediatrMock = new Mock<IMediator>();
            mediatrMock.Setup(mediator =>
                    mediator.Send(It.IsAny<IRequest<RequestStatus>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new RequestStatus((queuedCmd.AppCommand as ICommandContextOwner).CommandContext.CommandId, Status.COMPLETED)));
            return mediatrMock;
        }

        private static Mock<IUserIdentityService> MockUserIdentityService(UserId signedInUser)
        {
            var mockUserIdentityService = new Mock<IUserIdentityService>();
            mockUserIdentityService.Setup(service => service.GetSignedInUserIdentity()).Returns(signedInUser);
            return mockUserIdentityService;
        }

        private static Mock<IHTTPQueuedCommandStatusStorage> MockQueuedCommandStatusStorage(SemaphoreSlim sem)
        {
            var mockQueuedCommandStatusStorage = new Mock<IHTTPQueuedCommandStatusStorage>();
            mockQueuedCommandStatusStorage
                .Setup(storage => storage.UpdateCommandStatus(It.IsAny<RequestStatus>(), It.IsAny<CommandId>()))
                .Callback(() => sem.Release());
            return mockQueuedCommandStatusStorage;
        }
    }
}