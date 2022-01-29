using Core.Common.Domain;
using Core.Common.Domain.Users;
using Users.Domain;
using Users.Domain.Repositories;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    internal class MsSqlUserRepository : MsSqlESRepositoryBaseExceptionDecorator, IUserRepository
    {
        public MsSqlUserRepository(MsSqlConnectionSettings connectionSettings) : base(connectionSettings)
        {
        }

        public User AddUser(User user)
        {
            AddAggregate(user.PendingEvents, user.AggregateId.ToString(), user.Version, "User");
            return user;
        }

        public void DeleteUser(UserId userId)
        {
            RemoveAggregate(userId.Value.ToString());
        }

        public User? FindUser(UserId userId)
        {
            List<Event>? aggEvents = ReadEvents(userId);
            User? user = aggEvents != null ? User.FromEvents(aggEvents) : null;
            return user;
        }

        public void UpdateUser(User user)
        {
            UpdateAggregate(user.PendingEvents, user.AggregateId.ToString(), user.Version, "User");
        }
    }
}