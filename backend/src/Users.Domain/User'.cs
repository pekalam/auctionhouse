using Core.DomainFramework;
using ReflectionMagic;
using Users.Domain;
using Users.Domain.Events;
using Users.Domain.Shared;
using Users.DomainEvents;

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
            Credits = @event.InitialCredits;
        }

        private void ApplyEvent(CreditsAdded ev)
        {
            Credits += ev.CreditsCount;
        }
        private void ApplyEvent(CreditsWithdrawn ev)
        {
            _lockedFunds.Remove(_lockedFunds.First(l => l.Id.Value == ev.LockedFundsId));
            Credits -= ev.CreditsCount;
        }

        private void ApplyEvent(LockedFundsCreated ev)
        {
           _lockedFunds.Add(new LockedFunds(new LockedFundsId(ev.LockedFundsId), ev.Amount));
        }

    }
}