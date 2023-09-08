﻿using Common.Application;
using FunctionalTests.Commands;
using FunctionalTests.Tests.Auctions.CreateAuction;
using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Core.Model;
using System;
using System.Linq;
using Users.Application.Commands.SignUp;
using Xunit;
using Xunit.Abstractions;

namespace FunctionalTests.Tests.Users
{
    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class SignUpCommandTests : TestBase
    {
        public SignUpCommandTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async void SignUpCommand_creates_user_and_user_payments()
        {
            var cmd = new SignUpCommand("singup_test", "pass", "mail@mail.com");

            var requestStatus = await SendCommand(cmd);

            AssertEventual(new SignUpCommandProbe(this, requestStatus, cmd.Username).Check);
        }

        public override void Dispose()
        {
            base.Dispose();
            ReadModelDbContext.UsersReadModel.DeleteMany(Builders<UserRead>.Filter.Empty);
            CommandDbHelper.ClearUsersAuthenticationData();
        }
    }

    public class SignUpCommandProbe
    {
        private readonly TestBase _testBase;
        private readonly RequestStatus _requestStatus;
        private readonly string _userName;

        public SignUpCommandProbe(TestBase testBase, RequestStatus requestStatus, string userName)
        {
            _testBase = testBase;
            _requestStatus = requestStatus;
            _userName = userName;
        }

        public bool Check()
        {
            var (sagaCompleted, allEventsProcessed) = _testBase.SagaShouldBeCompletedAndAllEventsShouldBeProcessed(_requestStatus);
            var createdUser = _testBase.ReadModelDbContext.UsersReadModel.Find(u => u.UserIdentity.UserName == _userName).SingleOrDefault();
            var userPaymentsCreated = false;
            if (createdUser != null)
            {
                userPaymentsCreated = _testBase.ReadModelDbContext.UserPaymentsReadModel.Find(u => u.UserId == createdUser.UserIdentity.UserId).SingleOrDefault()
                != null;
            }
            return createdUser != null && sagaCompleted && allEventsProcessed && userPaymentsCreated;
        }
    }
}
