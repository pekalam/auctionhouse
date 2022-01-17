using Core.Common.Domain;

namespace Users.DomainEvents
{
    public class CreditsWithdrawn : Event
    {
        public decimal CreditsCount { get; }
        public Guid UserId { get; }

        public Guid LockedFundsId { get; }

        public CreditsWithdrawn(decimal creditsCount, Guid userId, Guid lockedFundsId) : base("creditsWithdrawn")
        {
            CreditsCount = creditsCount;
            UserId = userId;
            LockedFundsId = lockedFundsId;
        }
    }
}
