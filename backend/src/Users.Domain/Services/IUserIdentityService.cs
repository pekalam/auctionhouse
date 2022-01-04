using System;
using Core.Common.Domain.Users;

namespace Users.Domain.Services
{
    public interface IUserIdentityService
    {
        Guid GetSignedInUserIdentity();
    }
}
