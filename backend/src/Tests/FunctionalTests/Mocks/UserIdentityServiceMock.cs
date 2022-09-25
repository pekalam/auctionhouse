using Common.Application;
using System;

namespace FunctionalTests.Mocks
{
    internal class UserIdentityServiceMock : IUserIdentityService
    {
        public Guid UserId { get; set; }

        public UserIdentityServiceMock(Guid userId)
        {
            UserId = userId;
        }

        public Guid GetSignedInUserIdentity()
        {
            return UserId;
        }
    }
}
