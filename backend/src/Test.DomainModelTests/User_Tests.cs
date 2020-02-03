using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.Domain.Users.Events;
using Core.Common.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace Core.DomainModelTests
{

    [TestFixture]
    public class User_Tests
    {
        private User user;

        [SetUp]
        public void SetUp()
        {
            user = new User();
        }

        [Test]
        public void Register_when_valid_username_creates_user_identity()
        {
            var username = "test_username";

            user.Register(username);

            user.UserIdentity.Should().NotBeNull();
            user.UserIdentity.UserName.Should().Be(username);
            user.UserIdentity.UserId.Should().NotBeEmpty();
            user.PendingEvents.Count.Should().Be(1);
            user.PendingEvents.First().GetType().Should().Be(typeof(UserRegistered));
        }

        [Test]
        public void Register_called_twice_throws()
        {
            var username = "test_username";

            user.Register(username);
            user.MarkPendingEventsAsHandled();

            Assert.Throws<DomainException>(() => user.Register(username));
        }

        [Test]
        public void StartAuctionCreateSession_if_registered_creates_session()
        {
            var username = "test_username";

            user.Register(username);
            var session = user.UserIdentity.GetAuctionCreateSession();

            session.Should().NotBeNull();
        }

        [Test]
        public void StartAuctionCreateSession_if_not_registered_throws()
        {
            Assert.Throws<NullReferenceException>(() => user.UserIdentity.GetAuctionCreateSession());
        }

        [Test]
        public void AddCredits_adds_credits()
        {
            user.Register("test username");
            user.AddCredits(100);

            user.Credits.Should().Be(100);
            user.PendingEvents.Should().HaveCount(2);
            user.PendingEvents.Last().Should().BeOfType<CreditsAdded>();
        }

        [Test]
        public void AddCredits_when_not_registered_throws()
        {
            Assert.Throws<DomainException>(() => user.AddCredits(11));
        }
    }
}
