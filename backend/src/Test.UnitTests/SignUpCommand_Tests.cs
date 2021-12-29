using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Command;
using Core.Command.Exceptions;
using Core.Command.SignUp;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.Domain.Users.Events;
using Core.Common.EventBus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTests
{
    public class SignUpCommand_Tests
    {
        private bool verifyRegisteredEvent(IEnumerable<Event> @event)
        {
            var ev = @event.Select(e => e as UserRegistered)
                .ToList();
            return ev.Count == 1 && ev[0].UserId != Guid.Empty;
        }


        [Test]
        public void SignUp_when_too_weak_password_throws()
        {
            var username = "test";
            var password = "123";
            var email = "mail@mail.com";
            var command = new SignUpCommand(username, password, email);
            var mockEventBusService = new Mock<EventBusService>(Mock.Of<IEventBus>(), Mock.Of<IAppEventBuilder>());
            var commandHandler = new SignUpCommandHandler(mockEventBusService.Object,
                Mock.Of<IUserAuthenticationDataRepository>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<ILogger<SignUpCommandHandler>>());

            Assert.Throws<InvalidCommandException>(() => commandHandler.Handle(command, CancellationToken.None));
        }

        [Test]
        public void SignUp_when_valid_password_does_not_throw()
        {
            var username = "test";
            var password = "password";
            var email = "mail@mail.com";
            var command = new SignUpCommand(username, password, email);
            var mockEventBusService = new Mock<EventBusService>(Mock.Of<IEventBus>(), Mock.Of<IAppEventBuilder>());
            var commandHandler = new SignUpCommandHandler(mockEventBusService.Object,
                Mock.Of<IUserAuthenticationDataRepository>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<ILogger<SignUpCommandHandler>>());

            Assert.DoesNotThrow(() => commandHandler.Handle(command, CancellationToken.None));
        }

        [Test]
        public void SignUp_when_valid_params_returns_valid_user_identity_and_publishes_events()
        {
            var username = "test";
            var password = "pass";
            var email = "mail@mail.com";
            var command = new SignUpCommand(username, password, email);
            var authRepo = new Mock<IUserAuthenticationDataRepository>();

            UserId userIdentity = null;
            var mockEventBusService = new Mock<EventBusService>(Mock.Of<IEventBus>(), Mock.Of<IAppEventBuilder>());
            mockEventBusService.Setup(f => f.Publish(
                    It.Is<IEnumerable<Event>>(ev => verifyRegisteredEvent(ev)),
                    It.IsAny<Core.Common.EventBus.CorrelationId>(),
                    command)
                )
                .Callback((IEnumerable<Event> evs, Core.Common.EventBus.CorrelationId id, CommandBase cmd) =>
                {
                    userIdentity = (evs.First() as UserRegistered).UserId;
                })
                .Verifiable();

            var commandHandler = new SignUpCommandHandler(mockEventBusService.Object, authRepo.Object,
                Mock.Of<IUserRepository>(),
                Mock.Of<ILogger<SignUpCommandHandler>>());
            commandHandler.Handle(command, CancellationToken.None)
                .Wait();


            mockEventBusService.Verify(
                f => f.Publish(It.IsAny<IEnumerable<Event>>(), It.IsAny<Core.Common.EventBus.CorrelationId>(), command), Times.Once);
            userIdentity.Should()
                .NotBe(UserId.Empty);
        }

        [Test]
        public void SignUp_when_user_exists_throws()
        {
            var username = "test";
            var password = "pass";
            var email = "mail@mail.com";

            var stubAuthRepo = new Mock<IUserAuthenticationDataRepository>();
            stubAuthRepo
                .Setup(repository => repository.FindUserAuth(username))
                .Returns(new UserAuthenticationData()
                {
                    UserId = Guid.NewGuid(),
                    UserName = username,
                    Password = password
                });

            var command = new SignUpCommand(username, password, email);

            var mockEventBusService = new Mock<EventBusService>(Mock.Of<IEventBus>(), Mock.Of<IAppEventBuilder>());
            mockEventBusService
                .Setup(f => f.Publish(
                    It.Is<IEnumerable<Event>>(ev => verifyRegisteredEvent(ev)),
                    It.IsAny<Core.Common.EventBus.CorrelationId>(), command)
                )
                .Verifiable();

            var commandHandler = new SignUpCommandHandler(mockEventBusService.Object, stubAuthRepo.Object,
                Mock.Of<IUserRepository>(),
                Mock.Of<ILogger<SignUpCommandHandler>>());



            Assert.Throws<UsernameConflictException>(() =>
                commandHandler.Handle(command, CancellationToken.None)
                    .Wait());

            mockEventBusService.Verify(f => f.Publish(
                    It.Is<IEnumerable<Event>>(ev => verifyRegisteredEvent(ev)),
                    It.IsAny<Core.Common.EventBus.CorrelationId>(), command)
                , Times.Never);
        }
    }
}