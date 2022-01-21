using Xunit;
using Core.Common.Domain.Users;
using Users.Domain;
using Users.Domain.Shared;
using System;
using static Test.UsersDomain.UserTestContants;
using FluentAssertions;
using System.Linq;

namespace Test.UsersDomain
{
    public class User_Tests
    {
        [Fact]
        public void Locked_credits_should_be_subtracted_from_all_credits()
        {
            const decimal toLock = 1;

            var user = new GivenUser().Build();
            user.MarkPendingEventsAsHandled();

            var initialCredits = user.Credits;
            var lockedId = new LockedFundsId(Guid.NewGuid());
            user.LockCredits(lockedId, toLock);

            user.Credits.Should().Be(initialCredits - toLock);
            user.LockedFunds.Count.Should().Be(1);
            user.LockedFunds.First().Id.Value.Should().Be(lockedId.Value);
        }

        [Fact]
        public void WithdrawCredits_should_remove_locked_credits()
        {
            const decimal toLock = 1;

            var user = new GivenUser().Build();
            user.MarkPendingEventsAsHandled();

            var initialCredits = user.Credits;
            var lockedId = new LockedFundsId(Guid.NewGuid());
            user.LockCredits(lockedId, toLock);
            user.WithdrawCredits(lockedId);

            user.Credits.Should().Be(initialCredits - 1);
            user.LockedFunds.Count.Should().Be(0);
        }
    }

    public class UserTestContants
    {
        public const decimal CREDITS = 10m;
        public static readonly Username USERNAME = new("test");
        public static readonly UserPaymentsId? USERPAYMENTSID = new(Guid.NewGuid());
    }

    public class GivenUser
    {
        private decimal credits = CREDITS;
        private Username username = USERNAME;
        private UserPaymentsId? userPaymentsId = USERPAYMENTSID;

        public GivenUser WithUserPaymentsId(UserPaymentsId id)
        {
            userPaymentsId = id;
            return this;
        }

        public User Build()
        {
            var user = User.Create(username, credits);
            user.AssignUserPayments(userPaymentsId);
            return user;
        }
    }
}