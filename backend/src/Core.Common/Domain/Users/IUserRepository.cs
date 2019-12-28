namespace Core.Common.Domain.Users
{
    public interface IUserRepository
    {
        User AddUser(User user);
        User FindUser(UserIdentity userIdentity);
        void UpdateUser(User user);
    }
}
