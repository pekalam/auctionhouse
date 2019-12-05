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
                    "Data Source=.;Initial Catalog=es;Integrated Security=True;")
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
    }
}