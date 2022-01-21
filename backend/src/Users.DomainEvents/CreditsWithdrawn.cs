using Core.Common.Domain;

namespace Users.DomainEvents
{
    public class CreditsWithdrawn : Event
    {
        public decimal Amount { get; }
        public Guid UserId { get; }

        public Guid LockedFundsId { get; }

        public CreditsWithdrawn(decimal amount, Guid userId, Guid lockedFundsId) : base("creditsWithdrawn")
        {
            Amount = amount;
            UserId = userId;
            LockedFundsId = lockedFundsId;
        }
    }
}
