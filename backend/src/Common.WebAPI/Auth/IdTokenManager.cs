using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Common.WebAPI.Auth
{
    public interface IIdTokenManager
    {
        Task<bool> CheckIsDeactivated(string idToken, CancellationToken ct);
        Task DeactivateToken(string idToken, CancellationToken ct);
    }

    internal class IdTokenManager : IIdTokenManager
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<IdTokenManager> _logger;
        private readonly JwtSettings _jwtSettings;

        public IdTokenManager(IDistributedCache cache, ILogger<IdTokenManager> logger, JwtSettings jwtSettings)
        {
            _cache = cache;
            _logger = logger;
            _jwtSettings = jwtSettings;
        }

        private string GetKey(string idToken) => $"IdToken:{idToken}:deactivated";

        public async Task<bool> CheckIsDeactivated(string idToken, CancellationToken ct)
        {
            if (await _cache.GetAsync(GetKey(idToken), ct) == null)
            {
                return false;
            }
            _logger.LogDebug("Token {@idToken] was deactivated", idToken);
            // token was deactivated
            return true;
        }

        public async Task DeactivateToken(string idToken, CancellationToken ct)
        {
            if(await _cache.GetAsync(GetKey(idToken), ct) == null)
            {
                await _cache.SetStringAsync(GetKey(idToken), idToken, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_jwtSettings.ExpireTimeSec + 30), //instead of reading time left
                }, ct);
            }
        }
    }
}
