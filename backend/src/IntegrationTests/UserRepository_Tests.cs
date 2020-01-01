using Core.Common.Domain.Users;
using FluentAssertions;
using Infrastructure.Repositories.SQLServer;
using NUnit.Framework;

namespace IntegrationTests
{
    public class UserRepository_Tests
    {
        private IUserRepository userRepository;
        private User user;

        [SetUp]
        public void SetUp()
        {
            var serverOpt = new MsSqlConnectionSettings()
            {
                ConnectionString = TestContextUtils.GetParameterOrDefault("sqlserver",
                    "Data Source=.;Initial Catalog=AuctionhouseDatabase;Integrated Security=False;User ID=sa;PWD=Qwerty1234;")
            };
            userRepository = new MsSqlUserRepository(serverOpt);
            user = new User();
            user.Register("test");
        }

        [Test]
        public void Adduser_adds_user_and_Finduser_reads_it()
        {
            userRepository.AddUser(user);
            user.MarkPendingEventsAsHandled();

            var read = userRepository.FindUser(user.UserIdentity);

            read.Should().BeEquivalentTo(user);
        }

        [Test]
        public void FindUser_when_not_found_returns_null()
        {
            var read = userRepository.FindUser(user.UserIdentity);

            read.Should()
                .BeNull();
        }

        [Test]
        public void UpdateUser_updates_user_with_1_event_and_FindUser_reads_it()
        {
            userRepository.AddUser(user);
            user.MarkPendingEventsAsHandled();
            user.AddCredits(10);

            userRepository.UpdateUser(user);
            user.MarkPendingEventsAsHandled();

            var read = userRepository.FindUser(user.UserIdentity);
            read.Should().BeEquivalentTo(user);
        }

        [Test]
        public void UpdateUser_updates_user_with_more_than_1_event_and_FindUser_reads_it()
        {
            user.AddCredits(19);
            user.AddCredits(10);
            userRepository.UpdateUser(user);
            user.MarkPendingEventsAsHandled();

            var read = userRepository.FindUser(user.UserIdentity);
            read.Should().BeEquivalentTo(user);
        }
    }
}