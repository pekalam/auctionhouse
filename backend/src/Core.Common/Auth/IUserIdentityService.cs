using Core.Common.Domain.Users;

namespace Core.Common.Auth
{
    public interface IUserIdentityService
    {
        UserIdentity GetSignedInUserIdentity();
    }
}
