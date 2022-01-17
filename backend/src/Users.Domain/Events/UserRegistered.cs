using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class UserRegistered : Event
    {
        public UserId UserId { get; }

        public string Username { get; }

        public decimal InitialCredits { get; }

        public UserRegistered(UserId userId, string username, decimal initialCredits) : base("userRegistered")
        {
            UserId = userId;
            Username = username;
            InitialCredits = initialCredits;
        }
    }
}