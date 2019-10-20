using System;
using Core.Common.Domain.Users;
using EventStore.ClientAPI;

namespace Infrastructure.Adapters.Repositories.EventStore
{
    public class ESUserRepository : EventStoreRepositoryBase, IUserRepository
    {
        public ESUserRepository(ESConnectionContext esConnectionContext) : base(esConnectionContext)
        {
            
        }

        public User AddUser(User user)
        {
            AppendPendingEventsToStream(user.PendingEvents, ExpectedVersion.NoStream, user.AggregateId);
            return user;
        }

        public User FindUser(UserIdentity userIdentity)
        {
            var events = ReadEvents(userIdentity.UserId);
            var user = User.FromEvents(events);
            return user;
        }

        protected override string GetStreamId(Guid entityId) => $"{nameof(User)}-{entityId}";
    }
}
