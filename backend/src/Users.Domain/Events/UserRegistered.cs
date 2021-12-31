using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class UserRegistered : Event
    {
        public UserId UserId { get; }

        public string Username { get; }

        public UserRegistered(UserId userId, string username) : base("userRegistered")
        {
            UserId = userId;
            Username = username;
        }
    }
}