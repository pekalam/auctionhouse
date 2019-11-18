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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Web.Exceptions;

namespace Web.Adapters
{
    public class MyContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic| BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .Select(p => base.CreateProperty(p, memberSerialization))
                .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(f => base.CreateProperty(f, memberSerialization)))
                .ToList();
            props.ForEach(p => { p.Writable = true; p.Readable = true; });
            return props;
        }
    }

    public class AuctionCreateSessionService : IAuctionCreateSessionService
    {
        private string GetSessionKey(UserIdentity userIdentity) => $"user-{userIdentity.UserId}";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserIdentityService _userIdentityService;
        private readonly ILogger<AuctionCreateSessionService> _logger;

        public AuctionCreateSessionService(IHttpContextAccessor httpContextAccessor, IUserIdentityService userIdentityService, ILogger<AuctionCreateSessionService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _userIdentityService = userIdentityService;
            _logger = logger;
        }

        private byte[] SerializeSession(AuctionCreateSession session)
        {
            var json = JsonConvert.SerializeObject(session, new JsonSerializerSettings()
            {
                ContractResolver = new MyContractResolver()
            });
            return Encoding.UTF8.GetBytes(json);
        }

        private AuctionCreateSession DeserializeSession(byte[] session)
        {
            var json = Encoding.UTF8.GetString(session);
            var deserialized = JsonConvert.DeserializeObject<AuctionCreateSession>(json, new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new MyContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                

            });
            return deserialized;
        }

        private UserIdentity GetSignedInUserIdentity()
        {
            var userIdnIdentity = _userIdentityService.GetSignedInUserIdentity();
            if (userIdnIdentity == null)
            {
                throw new ApiException(HttpStatusCode.Unauthorized, "not signed in");
            }

            return userIdnIdentity;
        }

        public AuctionCreateSession GetSessionForSignedInUser()
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

        public void SaveSessionForSignedInUser(AuctionCreateSession session)
        {
            var user = GetSignedInUserIdentity();
            var httpContext = _httpContextAccessor.HttpContext;
            httpContext.Session.Set(GetSessionKey(user), SerializeSession(session));
        }

        public void RemoveSessionForSignedInUser()
        {
            var user = GetSignedInUserIdentity();
            _httpContextAccessor.HttpContext.Session.Remove(GetSessionKey(user));
        }

        public bool SessionExists()
        {
            var user = GetSignedInUserIdentity();
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext.Session.Keys.Contains(GetSessionKey(user));
        }
    }
}