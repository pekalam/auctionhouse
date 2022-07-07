using System;
using Users.Domain.Auth;

namespace Users.Domain.Repositories
{
    public interface IUserAuthenticationDataRepository
    {
        UserAuthenticationData FindUserAuthById(Guid id);
        UserAuthenticationData FindUserAuth(string userName);
        UserAuthenticationData AddUserAuth(UserAuthenticationData userAuthenticationData);
        UserAuthenticationData FindUserAuthByEmail(string email);
        void UpdateUserAuth(UserAuthenticationData userAuthenticationData);
        void DeleteUserAuth(Guid id);
    }
}