using Core.DomainFramework;
using ReflectionMagic;
using Users.Domain;
using Users.Domain.Events;

namespace Core.Common.Domain.Users
{
    public partial class User
    {
        protected override void Apply(Event @event)
        {
            try
            {
                this.AsDynamic().ApplyEvent(@event);
            }
            catch (Exception)
            {
                throw new DomainException($"Unrecognized event: {@event.EventName}");
            }
        }

        protected override UserUpdateEventGroup CreateUpdateEventGroup()
        {
            return new UserUpdateEventGroup();
        }

        private void ApplyEvent(UserRegistered @event)
        {
            AggregateId = @event.UserId;
            Username = new Username(@event.Username);
        }

        private void ApplyEvent(CreditsAdded ev) => AddCredits(ev.CreditsCount);
        private void ApplyEvent(CreditsReturned ev) => ReturnCredits(ev.CreditsCount);
        private void ApplyEvent(CreditsWithdrawn ev) => WithdrawCredits(ev.CreditsCount);
        private void ApplyEvent(CreditsCanceled ev) => CancelCredits(ev.Ammount);

    }
}