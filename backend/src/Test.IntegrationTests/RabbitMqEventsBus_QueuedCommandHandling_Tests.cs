using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Command;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusService;
using Infrastructure.Services.EventBus;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace IntegrationTests
{
    public class TestQueuedCommandHandler : CommandHandlerBase<QueuedCommandBase>
    {
        public TestQueuedCommandHandler(ILogger logger) : base(logger)
        {
        }

        protected override Task<RequestStatus> HandleCommand(QueuedCommandBase request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class QueuedCommandBase : CommandBase
    {
        public int X { get; set; }
    }


    public class RabbitMqEventBusTestUtils
    {
        public static Lazy<RabbitMqEventBus> Bus { get; } = new Lazy<RabbitMqEventBus>(() =>
        {
            var bus = new RabbitMqEventBus(new RabbitMqSettings()
            {
                ConnectionString =
                    TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
            }, Mock.Of<ILogger<RabbitMqEventBus>>());
            return bus;
        });
    }

    public class RabbitMqEventsBus_QueuedCommandHandling_Tests
    {



        [Test]
        public void METHOD()
        {
            var signedInUser = UserId.New();
            var cmd = new QueuedCommandBase()
            {
                X = 1,
                CommandContext = new CommandContext(){CorrelationId = new CorrelationId(Guid.NewGuid().ToString()),User = signedInUser},
                WSQueued = true
            };
            var sem = new SemaphoreSlim(0, 1);

            var mockUserIdentityService = new Mock<IUserIdentityService>();
            mockUserIdentityService.Setup(service => service.GetSignedInUserIdentity()).Returns(signedInUser);

            var mockRequestStatusService = new Mock<IRequestStatusService>();


            var mediatrMock = new Mock<IMediator>();
            mediatrMock.Setup(mediator =>
                    mediator.Send(It.IsAny<IRequest<RequestStatus>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new RequestStatus(cmd.CommandContext.CorrelationId, Status.COMPLETED)));

            var testQueuedCommandHandler = new Mock<WSQueuedCommandHandler>(mockRequestStatusService.Object, mediatrMock.Object, Mock.Of<ILogger<WSQueuedCommandHandler>>());
            testQueuedCommandHandler.Setup(handler => handler.Handle(It.IsAny<QueuedCommandBase>()))
                .Callback(() => sem.Release())
                .CallBase();

            var mockImplProvider = new Mock<IImplProvider>();
            mockImplProvider.Setup(provider => provider.Get<IUserIdentityService>()).Returns(mockUserIdentityService.Object);
            mockImplProvider.Setup(provider => provider.Get<WSQueuedCommandHandler>()).Returns(testQueuedCommandHandler.Object);

            RabbitMqEventBusTestUtils.Bus.Value.CancelCommandSubscriptions();
            RabbitMqEventBusTestUtils.Bus.Value.InitCommandSubscribers("Test.IntegrationTests", mockImplProvider.Object);


            RabbitMqEventBusTestUtils.Bus.Value.Send(cmd);

            if (!sem.Wait(TimeSpan.FromSeconds(60))) { Assert.Fail(); };

            mockRequestStatusService.Verify(service => service.TrySendRequestFailureToUser(
                It.IsAny<string>(), It.IsAny<CorrelationId>(), It.IsAny<Guid>(),
                null
            ), Times.Never());

            Assert.AreEqual(0, sem.CurrentCount);
        }
    }
}