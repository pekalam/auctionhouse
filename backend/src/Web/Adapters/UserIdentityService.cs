using System;
using System.Net;
using System.Security.Claims;
using Core.Common.Auth;
using Core.Common.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Web.Exceptions;

namespace Web.Adapters
{
    public class UserIdentityService : IUserIdentityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserIdentityService> _logger;

        public UserIdentityService(IHttpContextAccessor httpContextAccessor, ILogger<UserIdentityService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public UserIdentity GetSignedInUserIdentity()
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var id = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Sid);
            if (id == null)
            {
                _logger.LogDebug($"User not signed in (url: {_httpContextAccessor.HttpContext.Request.GetDisplayUrl()})");
                throw new ApiException(HttpStatusCode.Unauthorized, "Not signed in");
            }
            Guid userId;
            if (Guid.TryParse(id, out userId))
            {
                return new UserIdentity() {UserId = userId, UserName = userName};
            }
            else
            {
                _logger.LogError($"Cannot parse GUID of user: {userName}");
                throw new ApiException(HttpStatusCode.InternalServerError, "Cannot parse guid");
            }
        }
    }
}
