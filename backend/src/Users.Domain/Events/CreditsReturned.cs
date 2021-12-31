using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class CreditsReturned : Event
    {
        public decimal CreditsCount { get; }
        public Guid UserIdentity { get; }

        public CreditsReturned(decimal creditsCount, Guid userIdentity) : base("creditsReturned")
        {
            CreditsCount = creditsCount;
            UserIdentity = userIdentity;
        }
    }
}
