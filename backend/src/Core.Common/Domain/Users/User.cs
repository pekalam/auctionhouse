using Core.Common.Domain.Users.Events;

namespace Core.Common.Domain.Users
{
    public partial class User : AggregateRoot<User>
    {
        public UserIdentity UserIdentity { get; private set; }

        public User()
        {
            
        }

        private object CheckIsRegistered() => UserIdentity ?? throw new DomainException($"User not registered");

        public void Register(string username)
        {
            if (UserIdentity != null)
            {
                throw new DomainException($"User {username} is already registered");
            }
            UserIdentity = new UserIdentity()
            {
                UserId = AggregateId,
                UserName = username
            };
            AddEvent(new UserRegistered(UserIdentity));
        }
    }
}
