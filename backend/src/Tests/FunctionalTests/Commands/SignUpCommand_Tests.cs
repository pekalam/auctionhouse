using MongoDB.Driver;
using ReadModel.Core.Model;
using System.Linq;
using Users.Application.Commands.SignUp;
using Xunit;
using Xunit.Abstractions;

namespace FunctionalTests.Commands
{
    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class SignUpCommandTests : TestBase
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
                var (sagaCompleted, allEventsProcessed) = SagaShouldBeCompletedAndAllEventsShouldBeProcessed(requestStatus);
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

        public override void Dispose()
        {
            base.Dispose();
            ReadModelDbContext.UsersReadModel.DeleteMany(Builders<UserRead>.Filter.Empty);
        }
    }
}
