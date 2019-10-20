using System;

namespace Core.Common.Auth
{
    public interface IUserAuthenticationDataRepository
    {
        UserAuthenticationData FindUserAuthById(Guid id);
        UserAuthenticationData FindUserAuth(string userName);
        UserAuthenticationData AddUserAuth(UserAuthenticationData userAuthenticationData);
    }
}