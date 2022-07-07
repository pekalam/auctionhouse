using Core.Common.Domain;

namespace Users.Domain.Events
{
    public class UserRemoved : Event
    {
        public UserRemoved() : base("userRemoved")
        {
        }
    }
}