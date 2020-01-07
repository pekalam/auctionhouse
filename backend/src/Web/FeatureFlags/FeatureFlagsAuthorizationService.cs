using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Services.SchedulerService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Web.FeatureFlags
{
    public class FeatureFlagsAuthorizationSettings
    {
        public string ApiKey { get; set; }
    }

    public class FeatureFlagsAuthorizationService
    {
        public static bool Authorize(StringValues xApiKeyValues, FeatureFlagsAuthorizationSettings authSettings, ILogger logger)
        {
            if (xApiKeyValues.Count == 0 || string.IsNullOrWhiteSpace(xApiKeyValues[0]) || xApiKeyValues[0] != authSettings.ApiKey)
            {
                logger.LogDebug($"Invalid {xApiKeyValues[0]} header value");
                return false;
            }
            logger.LogInformation($"FeatureFlagsAuthorizationService successfully authorized api-key: {xApiKeyValues[0]}");
            return true;
        }
    }
}
