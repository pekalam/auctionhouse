using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Command.SignUp;
using Core.Common.ApplicationServices;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.Domain.Users.Events;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Common.Interfaces;
using FluentAssertions;
using Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Infrastructure.UnitTests
{
    public class Tests
    {
        private bool verifyRegisteredEvent(IEnumerable<Event> @event)
        {
            var ev = @event.Select(e => e as UserRegistered).ToList();
            return ev.Count == 1 && ev[0].UserIdentity.UserName == "test"
                                 && ev[0].UserIdentity.UserId != Guid.Empty;
        }

        private UsertAuthDbContext PrepareFakeDbContext()
        {
            var dbContextOpt = new DbContextOptionsBuilder<UsertAuthDbContext>()
                .UseInMemoryDatabase("test_db")
                .Options;
            var dbContext = new UsertAuthDbContext(dbContextOpt);
            dbContext.Database.EnsureDeleted();
            return dbContext;
        }

        class StubEventBus : IEventBus
        {
            public void Publish<T>(IAppEvent<T> @event) where T : Event
            {
            }

            public void Publish<T>(IEnumerable<IAppEvent<T>> events) where T : Event
            {
            }
        }

        [Test]
        public void SignUp_called_first_time_with_valid_params_returns_valid_user_identity_and_publishes_events()
        {
            var username = "test";
            var password = "pass";

            var stubDbContext = PrepareFakeDbContext();
            var authRepo = new UserAuthenticationDataRepository(stubDbContext);

            var command = new SignUpCommand(username, password, new CorrelationId("123"));

            UserIdentity userIdentity = null;
            var mockEventBusService = new Mock<EventBusService>(Mock.Of<IEventBus>(), Mock.Of<IAppEventBuilder>());
            mockEventBusService.Setup(f => f.Publish(
                It.Is<IEnumerable<Event>>(ev => verifyRegisteredEvent(ev)),
                It.IsAny<CorrelationId>(),
                command)
            ).Callback((IEnumerable<Event> evs, CorrelationId id, ICommand cmd) =>
            {
                userIdentity = (evs.First() as UserRegistered).UserIdentity;
            }).Verifiable();

            var mockUserIdentityService = new SignUpCommandHandler(mockEventBusService.Object, authRepo,
                Mock.Of<IEventSignalingService>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<ILogger<SignUpCommandHandler>>());
            mockUserIdentityService.Handle(command, CancellationToken.None).Wait();

            stubDbContext.UserAuth.Count().Should().Be(1);
            mockEventBusService.Verify(
                f => f.Publish(It.IsAny<IEnumerable<Event>>(), It.IsAny<CorrelationId>(), command), Times.Once);
            userIdentity.UserName.Should().Be(username);
            userIdentity.UserId.Should().NotBe(Guid.Empty);
        }

        [Test]
        public void SignUp_called_second_time_with_valid_params_throws()
        {
            var username = "test";
            var password = "pass";

            var stubDbContext = PrepareFakeDbContext();
            var authRepo = new UserAuthenticationDataRepository(stubDbContext);
            var command = new SignUpCommand(username, password, new CorrelationId("123"));
            var mockEventBusService = new Mock<EventBusService>(Mock.Of<IEventBus>(), Mock.Of<IAppEventBuilder>());
            mockEventBusService.Setup(f => f.Publish(
                It.Is<IEnumerable<Event>>(ev => verifyRegisteredEvent(ev)),
                It.IsAny<CorrelationId>(), command)
            ).Verifiable();

            var mockUserIdentityService = new SignUpCommandHandler(mockEventBusService.Object, authRepo,
                Mock.Of<IEventSignalingService>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<ILogger<SignUpCommandHandler>>());
            mockUserIdentityService.Handle(command, CancellationToken.None).Wait();

            Assert.Throws<UsernameConflictException>(() =>
                mockUserIdentityService.Handle(command, CancellationToken.None).Wait());
        }
    }
}