namespace Core.Common.Domain.Users.Events
{
    public class UserRemoved : Event
    {
        public UserRemoved() : base("userRemoved")
        {
        }
    }
}