using Core.Common.Domain;

namespace Users.DomainEvents
{
    public class UserCreditsLockedForBuyNowAuction : Event
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }

        public UserCreditsLockedForBuyNowAuction() : base("userCreditsLockedForBuyNowAuction")
        {
        }
    }

    public class UserCreditsFailedToLockForBuyNowAuction : Event
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }

        public UserCreditsFailedToLockForBuyNowAuction() : base("userCreditsFailedToLockForBuyNowAuction")
        {
        }
    }
}