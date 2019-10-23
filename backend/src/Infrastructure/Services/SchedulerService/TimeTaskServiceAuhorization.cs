using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Infrastructure.Services.SchedulerService
{
    public class TimeTaskServiceAuhorization
    {
        public static bool Authorize(StringValues xApiKeyValues, TimeTaskServiceSettings serviceSettings, ILogger logger)
        {
            if (xApiKeyValues.Count == 0 || string.IsNullOrWhiteSpace(xApiKeyValues[0]) || xApiKeyValues[0] != serviceSettings.ApiKey)
            {
                logger.LogDebug($"Invalid {xApiKeyValues[0]} header value");
                return false;
            }
            logger.LogDebug($"Successful authentication: {xApiKeyValues[0]}");
            return true;
        }
    }
}