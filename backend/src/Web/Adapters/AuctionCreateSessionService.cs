using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Core.Common.Auth;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Web.Exceptions;

namespace Web.Adapters
{
    public class AuctionCreateSessionService : IAuctionCreateSessionService
    {
        private string GetSessionKey(UserId userIdentity) => $"user-{userIdentity}";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserIdentityService _userIdentityService;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<AuctionCreateSessionService> _logger;

        public AuctionCreateSessionService(IHttpContextAccessor httpContextAccessor, IUserIdentityService userIdentityService, IDistributedCache distributedCache, ILogger<AuctionCreateSessionService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _userIdentityService = userIdentityService;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        private byte[] SerializeSession(AuctionCreateSession session)
        {
            var json = JsonConvert.SerializeObject(session, new JsonSerializerSettings()
            {
            });
            return Encoding.UTF8.GetBytes(json);
        }

        private AuctionCreateSession DeserializeSession(byte[] session)
        {
            var json = Encoding.UTF8.GetString(session);
            var deserialized = JsonConvert.DeserializeObject<AuctionCreateSession>(json, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Include,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });
            return deserialized;
        }

        private UserId GetSignedInUserIdentity()
        {
            var userIdnIdentity = _userIdentityService.GetSignedInUserIdentity();
            if (userIdnIdentity == null)
            {
                throw new ApiException(HttpStatusCode.Unauthorized, "not signed in");
            }

            return userIdnIdentity;
        }

        public AuctionCreateSession GetExistingSession()
        {
            var user = GetSignedInUserIdentity();
            var httpContext = _httpContextAccessor.HttpContext;
            if (!httpContext.Session.Keys.Contains(GetSessionKey(user)))
            {
                throw new Exception($"Cannot find session with key:{GetSessionKey(user)}");
            }
            else
            {
                var auctionCreateSessionBytes = httpContext.Session.Get(GetSessionKey(user));
                var session = DeserializeSession(auctionCreateSessionBytes);
                return session;
            }
        }

        public AuctionCreateSession StartAndSaveNewSession()
        {
            var user = GetSignedInUserIdentity();
            _logger.LogDebug("Creating new AuctionCreateSession for {@user}", user);
            var newSession = new AuctionCreateSession(user);
            SaveSession(newSession);
            return newSession;
        }

        public void SaveSession(AuctionCreateSession session)
        {
            var user = GetSignedInUserIdentity();
            var key = GetSessionKey(user);
            var httpContext = _httpContextAccessor.HttpContext;
            _logger.LogDebug("Saving modified AuctionCreateSession {@session} for {@user} with key: {key}", session, user, key);
            httpContext.Session.Set(key, SerializeSession(session));
        }

        public void RemoveSession()
        {
            var user = GetSignedInUserIdentity();
            var key = GetSessionKey(user);
            _logger.LogDebug("Removing AuctionCreateSession for {@user} with key {key}", user, key);
            _httpContextAccessor.HttpContext.Session.Remove(key);
        }

        public bool SessionExists()
        {
            var user = GetSignedInUserIdentity();
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext.Session.Keys.Contains(GetSessionKey(user));
        }
    }
}