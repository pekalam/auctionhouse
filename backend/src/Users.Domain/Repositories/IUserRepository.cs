using Core.Common.Domain.Users;

namespace Users.Domain.Repositories
{
    public interface IUserRepository
    {
        User AddUser(User user);
        User FindUser(UserId userId);
        void UpdateUser(User user);
    }
}
