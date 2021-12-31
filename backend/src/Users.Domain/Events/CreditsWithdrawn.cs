using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class CreditsWithdrawn : Event
    {
        public decimal CreditsCount { get; }
        public Guid UserIdentity { get; }

        public CreditsWithdrawn(decimal creditsCount, Guid userIdentity) : base("creditsWithdrawn")
        {
            CreditsCount = creditsCount;
            UserIdentity = userIdentity;
        }
    }
}
