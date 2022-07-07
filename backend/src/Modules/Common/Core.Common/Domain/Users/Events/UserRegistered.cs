namespace Core.Common.Domain.Users.Events
{
    public class UserRegistered : Event
    {
        public UserId UserId { get; }

        public string Username { get; }

        public UserRegistered(UserId userId, string username) : base(EventNames.UserRegistered)
        {
            UserId = userId;
            Username = username;
        }
    }
}