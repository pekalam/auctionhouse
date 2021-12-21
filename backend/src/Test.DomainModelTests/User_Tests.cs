using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Core.Common.Domain;
using Core.Common.Domain.AuctionCreateSession;
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
        private readonly Username _username = new Username("test_username");
        private User user;

        [SetUp]
        public void SetUp()
        {
            user = User.Create(new Username(_username));
        }

        [Test]
        public void Register_when_valid_username_creates_user_identity()
        {
            user.Username.Should().Be(_username);
            user.AggregateId.Should().NotBe(UserId.Empty);
            user.PendingEvents.Count.Should().Be(1);
            user.PendingEvents.First().GetType().Should().Be(typeof(UserRegistered));
        }

        [Test]
        public void StartAuctionCreateSession_if_registered_creates_session()
        {
            var session = AuctionCreateSession.CreateSession(user.AggregateId);

            session.Should().NotBeNull();
        }

        [Test]
        public void AddCredits_adds_credits()
        {
            user.AddCredits(100);

            user.Credits.Should().Be(100);
            user.PendingEvents.Should().HaveCount(2);
            user.PendingEvents.Last().Should().BeOfType<CreditsAdded>();
        }
    }
}
