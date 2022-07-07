namespace Core.Common.Domain.Users
{
    public class UserUpdateEventGroup : UpdateEventGroup<UserId>
    {
        public UserUpdateEventGroup() : base("userUpdated")
        {
        }
    }
}