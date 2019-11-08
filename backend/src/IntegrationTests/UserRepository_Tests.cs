using Core.Common.Domain.Users;
using FluentAssertions;
using Infrastructure.Repositories.EventStore;
using NUnit.Framework;

namespace IntegrationTests
{
    public class UserRepository_Tests
    {
        private ESUserRepository userRepository;
        private User user;

        [SetUp]
        public void SetUp()
        {
            var esConnectionContext = new ESConnectionContext(new EventStoreConnectionSettings()
            {
                IPAddress = TestContextUtils.GetParameterOrDefault("eventstore-connection-string", "localhost"),
                Port = 1113
            });
            esConnectionContext.Connect();
            userRepository = new ESUserRepository(esConnectionContext);
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