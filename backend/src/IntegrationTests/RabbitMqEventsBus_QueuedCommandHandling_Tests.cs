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
    public class TestQueuedCommandHandler : CommandHandlerBase<QueuedCommand>
    {
        public TestQueuedCommandHandler(ILogger logger) : base(logger)
        {
        }

        protected override Task<RequestStatus> HandleCommand(QueuedCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class QueuedCommand : ICommand
    {
        public int X { get; set; }
    }

    public class RabbitMqEventsBus_QueuedCommandHandling_Tests
    {



        [Test]
        public void METHOD()
        {
            var signedInUser = new UserIdentity(Guid.NewGuid(), "test");
            var cmd = new QueuedCommand()
            {
                X = 1,
                CommandContext = new CommandContext(){CorrelationId = new CorrelationId(Guid.NewGuid().ToString()),User = signedInUser},
                WSQueued = true
            };
            var sem = new SemaphoreSlim(0, 2);
            var bus = new RabbitMqEventBus(new RabbitMqSettings()
            {
                ConnectionString =
                    TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
            }, Mock.Of<ILogger<RabbitMqEventBus>>());


            var mockUserIdentityService = new Mock<IUserIdentityService>();
            mockUserIdentityService.Setup(service => service.GetSignedInUserIdentity()).Returns(signedInUser);

            var mockRequestStatusService = new Mock<IRequestStatusService>();
            mockRequestStatusService.Setup(service => service.TrySendRequestCompletionToUser(
                It.IsAny<string>(), It.IsAny<CorrelationId>(), It.IsAny<UserIdentity>(),
                null
            )).Callback(
                () =>
                {
                    sem.Release();
                });


            var mediatrMock = new Mock<IMediator>();
            mediatrMock.Setup(mediator =>
                    mediator.Send(It.IsAny<IRequest<RequestStatus>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new RequestStatus(cmd.CommandContext.CorrelationId, Status.COMPLETED)));

            var testQueuedCommandHandler = new Mock<WSQueuedCommandHandler>(mockRequestStatusService.Object, mediatrMock.Object);
            testQueuedCommandHandler.Setup(handler => handler.Handle(It.IsAny<QueuedCommand>()))
                .Callback(() => sem.Release())
                .CallBase();

            var mockImplProvider = new Mock<IImplProvider>();
            mockImplProvider.Setup(provider => provider.Get<IUserIdentityService>()).Returns(mockUserIdentityService.Object);
            mockImplProvider.Setup(provider => provider.Get<WSQueuedCommandHandler>()).Returns(testQueuedCommandHandler.Object);

            bus.InitCommandSubscribers("IntegrationTests", mockImplProvider.Object);

          
            bus.Send(cmd);

            if (!sem.Wait(TimeSpan.FromSeconds(10))) { Assert.Fail(); };

            if (!sem.Wait(TimeSpan.FromSeconds(10))){Assert.Fail();};
            

            mockRequestStatusService.Verify(service => service.TrySendRequestFailureToUser(
                It.IsAny<string>(), It.IsAny<CorrelationId>(), It.IsAny<UserIdentity>(),
                null
            ), Times.Never());

            Assert.AreEqual(0, sem.CurrentCount);
        }
    }
}