using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class CreditsCanceled : Event
    {
        public decimal Ammount { get; }
        public Guid User { get; }

        public CreditsCanceled(decimal ammount, Guid user) : base("creditsCanceled")
        {
            Ammount = ammount;
            User = user;
        }
    }
}
