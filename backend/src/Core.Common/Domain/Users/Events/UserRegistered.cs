namespace Core.Common.Domain.Users.Events
{
    public class UserRegistered : Event
    {
        public UserIdentity UserIdentity { get; }

        public UserRegistered(UserIdentity userIdentity) : base(EventsNames.UserRegisteredEventName)
        {
            UserIdentity = userIdentity;
        }
    }
}