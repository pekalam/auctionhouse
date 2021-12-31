using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class CreditsAdded : Event
    {
        public decimal CreditsCount { get; }
        public Guid UserIdentity { get; }

        public CreditsAdded(decimal creditsCount, Guid userIdentity) : base("creditsAdded")
        {
            CreditsCount = creditsCount;
            UserIdentity = userIdentity;
        }
    }
}
