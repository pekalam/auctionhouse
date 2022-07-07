namespace Core.Common.Domain.Users
{
    public interface IUserRepository
    {
        User AddUser(User user);
        User FindUser(UserId userId);
        void UpdateUser(User user);
    }
}
