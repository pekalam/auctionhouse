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
using Core.Common.RequestStatusService;
using Infrastructure.Services.EventBus;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class RabbitMqEventBus_HTTPQueuedCommandHandling_Tests
    {
        [Test]
        public void METHOD()
        {
            var signedInUser = UserId.New();
            var cmd = new QueuedCommandBase()
            {
                X = 1,
                CommandContext = new CommandContext(){CorrelationId = new CorrelationId(Guid.NewGuid().ToString()), User = signedInUser},
                HttpQueued = true
            };
            var sem = new SemaphoreSlim(0, 2);


            var mockUserIdentityService = new Mock<IUserIdentityService>();
            mockUserIdentityService.Setup(service => service.GetSignedInUserIdentity()).Returns(signedInUser);

            var mockQueuedCommandStatusStorage = new Mock<IHTTPQueuedCommandStatusStorage>();
            mockQueuedCommandStatusStorage
                .Setup(storage => storage.UpdateCommandStatus(It.IsAny<RequestStatus>(), It.IsAny<CommandBase>()))
                .Callback(() => sem.Release());

            var mediatrMock = new Mock<IMediator>();
            mediatrMock.Setup(mediator =>
                    mediator.Send(It.IsAny<IRequest<RequestStatus>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new RequestStatus(cmd.CommandContext.CorrelationId, Status.COMPLETED)));

            var testQueuedCommandHandler = new Mock<HTTPQueuedCommandHandler>(
                mockQueuedCommandStatusStorage.Object,
                mediatrMock.Object,
                Mock.Of<ILogger<HTTPQueuedCommandHandler>>()
            );
            testQueuedCommandHandler.Setup(handler => handler.Handle(It.IsAny<QueuedCommandBase>()))
                .Callback(() => sem.Release())
                .CallBase();

            var mockImplProvider = new Mock<IImplProvider>();
            mockImplProvider.Setup(provider => provider.Get<IUserIdentityService>())
                .Returns(mockUserIdentityService.Object);
            mockImplProvider.Setup(provider => provider.Get<HTTPQueuedCommandHandler>())
                .Returns(testQueuedCommandHandler.Object);

            RabbitMqEventBusTestUtils.Bus.Value.CancelCommandSubscriptions();
            RabbitMqEventBusTestUtils.Bus.Value.InitCommandSubscribers("Test.IntegrationTests", mockImplProvider.Object);

            RabbitMqEventBusTestUtils.Bus.Value.Send(cmd);

            if (!sem.Wait(TimeSpan.FromSeconds(60)))
            {
                Assert.Fail();
            }

            if (!sem.Wait(TimeSpan.FromSeconds(60)))
            {
                Assert.Fail();
            }

            mockQueuedCommandStatusStorage
                .Verify(storage => storage.UpdateCommandStatus(It.IsAny<RequestStatus>(), It.IsAny<CommandBase>()), Times.Once());

            Assert.AreEqual(0, sem.CurrentCount);
        }
    }
}