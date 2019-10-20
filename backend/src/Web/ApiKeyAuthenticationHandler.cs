using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Infrastructure.Adapters.Services.SchedulerService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Web
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public static readonly string Scheme = "X-API-Key";
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private const string ApiKeyHeader = "X-API-Key";
        private ILogger<ApiKeyAuthenticationHandler> _logger;
        private readonly TimeTaskServiceSettings _taskServiceSettings;

        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, TimeTaskServiceSettings taskServiceSettings) : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();
            _taskServiceSettings = taskServiceSettings;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.TryGetValue(ApiKeyHeader, out var apiHeaderValues) && TimeTaskServiceAuhorization.Authorize(apiHeaderValues, _taskServiceSettings, _logger))
            {
                var claimsIdentity = new ClaimsIdentity(new []{new Claim(ClaimTypes.Sid, apiHeaderValues[0]), new Claim(ClaimTypes.Role, "TimeTaskService"),  });
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                var authTicket = new AuthenticationTicket(claimsPrincipal, ApiKeyAuthenticationOptions.Scheme);
                return Task.FromResult(AuthenticateResult.Success(authTicket));
            }
            else
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }
        }
    }
}
