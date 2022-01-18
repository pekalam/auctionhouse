using Auctions.Domain;
using Auctions.Domain.Services;
using Common.Application;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace Auctionhouse.Command.Adapters
{
    internal class AuctionCreateSessionDto
    {
        public string?[] Size1Ids { get; set; }
        public string?[] Size2Ids { get; set; }
        public string?[] Size3Ids { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid OwnerId { get; set; }
    }

    internal static class AuctionCreateSessionAssembler
    {
        public static AuctionCreateSessionDto ToDto(AuctionCreateSession auctionCreateSession)
        {
            return new AuctionCreateSessionDto
            {
                DateCreated = auctionCreateSession.DateCreated,
                OwnerId = auctionCreateSession.OwnerId,
                Size1Ids = auctionCreateSession.SessionAuctionImages.Size1Ids.ToArray(),
                Size2Ids = auctionCreateSession.SessionAuctionImages.Size2Ids.ToArray(),
                Size3Ids = auctionCreateSession.SessionAuctionImages.Size3Ids.ToArray(),
            };
        }

        public static AuctionCreateSession FromDto(AuctionCreateSessionDto dto)
        {
            return new AuctionCreateSession(AuctionImages.FromSizeIds(dto.Size1Ids, dto.Size2Ids, dto.Size3Ids),
                dto.DateCreated, new(dto.OwnerId));
        }
    }

    internal class AuctionCreateSessionStore : IAuctionCreateSessionStore
    {
        private string GetSessionKey(UserId userIdentity) => $"user-{userIdentity}";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserIdentityService _userIdentityService;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<AuctionCreateSessionStore> _logger;

        public AuctionCreateSessionStore(IHttpContextAccessor httpContextAccessor, IUserIdentityService userIdentityService, IDistributedCache distributedCache,
            ILogger<AuctionCreateSessionStore> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _userIdentityService = userIdentityService;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        private byte[] SerializeSession(AuctionCreateSession session)
        {
            var dto = AuctionCreateSessionAssembler.ToDto(session);
            var json = JsonConvert.SerializeObject(dto, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
            });
            return Encoding.UTF8.GetBytes(json);
        }

        private AuctionCreateSession DeserializeSession(byte[] session)
        {
            var json = Encoding.UTF8.GetString(session);
            var deserialized = JsonConvert.DeserializeObject<AuctionCreateSessionDto>(json, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Include,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            });
            return AuctionCreateSessionAssembler.FromDto(deserialized);
        }

        private UserId GetSignedInUserIdentity()
        {
            var userIdnIdentity = _userIdentityService.GetSignedInUserIdentity();
            if (userIdnIdentity == null)
            {
                throw new NullReferenceException();
                //TODO
                //throw new ApiException(HttpStatusCode.Unauthorized, "not signed in");
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