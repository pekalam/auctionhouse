using System;

namespace Common.Application
{
    public interface IUserIdentityService
    {
        Guid GetSignedInUserIdentity();
    }
}
