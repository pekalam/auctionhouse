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
            this.AsDynamic().ApplyEvent(@event);
        }

        protected override UserUpdateEventGroup CreateUpdateEventGroup()
        {
            return new UserUpdateEventGroup();
        }
    }
}