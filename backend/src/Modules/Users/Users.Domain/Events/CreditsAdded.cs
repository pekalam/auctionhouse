using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class CreditsAdded : Event
    {
        public decimal CreditsCount { get; }
        public Guid UserId { get; }

        public CreditsAdded(decimal creditsCount, Guid userId) : base("creditsAdded")
        {
            CreditsCount = creditsCount;
            UserId = userId;
        }
    }
}
