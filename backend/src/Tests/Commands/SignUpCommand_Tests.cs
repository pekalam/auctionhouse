﻿using Common.Application;
using MongoDB.Driver;
using ReadModel.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Commands.SignUp;
using Xunit;
using Xunit.Abstractions;

namespace FunctionalTests.Commands
{
    public class SignUpCommandTests : TestBase, IDisposable
    {
        public SignUpCommandTests(ITestOutputHelper outputHelper) : base(outputHelper,
            "UserPayments.Application", "Users.Application")
        {
        }

        [Fact]
        public async void SignUpCommand_creates_user_and_user_payments()
        {
            var cmd = new SignUpCommand("test_signup", "pass", "mail@mail.com");

            var requestStatus = await SendCommand(cmd);

            AssertEventual(() =>
            {
                var (sagaCompleted, allEventsProcessed) = CheckSagaCompletedAndAllEventsProcessed(requestStatus);
                var createdUser = ReadModelDbContext.UsersReadModel.Find(u => u.UserIdentity.UserName == cmd.Username).SingleOrDefault();
                var userPaymentsCreated = false;
                if (createdUser != null)
                {
                    userPaymentsCreated = ReadModelDbContext.UserPaymentsReadModel.Find(u => u.UserId == createdUser.UserIdentity.UserId).SingleOrDefault()
                    != null;
                }
                return createdUser != null && sagaCompleted && allEventsProcessed && userPaymentsCreated;
            });
        }

        private (bool sagaCompleted, bool allEventsProcessed) CheckSagaCompletedAndAllEventsProcessed(RequestStatus requestStatus)
        {
            var eventConfirmations = SagaEventsConfirmationDbContext.SagaEventsConfirmations.FirstOrDefault(e => e.CommandId == requestStatus.CommandId.Id);
            var sagaCompleted = eventConfirmations?.Completed == true;
            var allEventsProcessed = false;
            if (eventConfirmations != null)
            {
                var eventsToProcess = SagaEventsConfirmationDbContext.SagaEventsToConfirm
                 .Where(e => e.CorrelationId == eventConfirmations.CorrelationId).ToList();

                allEventsProcessed = eventsToProcess.Count > 0 && eventsToProcess.All(e => e.Processed);
            }

            return (sagaCompleted, allEventsProcessed);
        }

        public override void Dispose()
        {
            base.Dispose();
            ReadModelDbContext.UsersReadModel.DeleteMany(Builders<UserRead>.Filter.Empty);
            TruncateReadModelNotificaitons(ServiceProvider);
        }
    }
}