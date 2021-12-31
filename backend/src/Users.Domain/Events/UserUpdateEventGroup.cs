using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class UserUpdateEventGroup : UpdateEventGroup<UserId>
    {
        public UserUpdateEventGroup() : base("userUpdated")
        {
        }
    }
}