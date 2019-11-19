using Core.Common.Domain.Users.Events;
using Core.Common.Exceptions;

namespace Core.Common.Domain.Users
{
    public partial class User
    {
        public override Event GetRemovedEvent()
        {
            return new UserRemoved();
        }

        protected override void Apply(Event @event)
        {
            if (@event is UserRegistered)
                Apply(@event as UserRegistered);
            else
            {
                throw new DomainException("Event not recognized");
            }
        }

        private void Apply(UserRegistered @event)
        {
            AggregateId = @event.UserIdentity.UserId;
            Register(@event.UserIdentity.UserName);
        }

    }
}