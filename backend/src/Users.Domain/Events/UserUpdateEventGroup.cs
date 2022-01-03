using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class UserUpdateEventGroup : UpdateEventGroup
    {
        public UserUpdateEventGroup() : base("userUpdated")
        {
        }
    }
}