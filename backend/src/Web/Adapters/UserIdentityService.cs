using System;
using System.Security.Claims;
using Core.Common.Auth;
using Core.Common.Domain.Users;
using Microsoft.AspNetCore.Http;

namespace Web.Adapters
{
    public class UserIdentityService : IUserIdentityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserIdentityService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserIdentity GetSignedInUserIdentity()
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var id = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Sid);
            if (id == null)
            {
                throw new ApiExcpetion("Not signed in");
            }
            Guid userId;
            if (Guid.TryParse(id, out userId))
            {
                return new UserIdentity() {UserId = userId, UserName = userName};
            }
            else
            {
                throw new ApiExcpetion("Cannot parse guid");
            }
        }
    }
}
