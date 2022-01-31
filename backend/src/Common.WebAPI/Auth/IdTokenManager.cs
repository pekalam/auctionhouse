using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public IdTokenManager(IDistributedCache cache, ILogger<IdTokenManager> logger)
        {
            _cache = cache;
            _logger = logger;
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
            await _cache.SetStringAsync(GetKey(idToken), idToken, ct);
        }
    }
}
