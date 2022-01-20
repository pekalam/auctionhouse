using Core.Common.Domain;

namespace Users.DomainEvents
{
    public class UserRegistered : Event
    {
        public Guid UserId { get; }

        public string Username { get; }

        public decimal InitialCredits { get; }

        public UserRegistered(Guid userId, string username, decimal initialCredits) : base("userRegistered")
        {
            UserId = userId;
            Username = username;
            InitialCredits = initialCredits;
        }
    }
}
