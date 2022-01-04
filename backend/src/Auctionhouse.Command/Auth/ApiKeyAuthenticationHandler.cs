namespace Auctionhouse.Command.Auth
{
    //public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    //{
    //    public static readonly string Scheme = "X-API-Key";
    //}

    //public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    //{
    //    private const string ApiKeyHeader = "X-API-Key";
    //    private readonly ILogger<ApiKeyAuthenticationHandler> _logger;
    //    private readonly TimeTaskServiceSettings _taskServiceSettings;
    //    private readonly FeatureFlagsAuthorizationSettings _featureFlagsAuthorizationSettings;

    //    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ILogger<ApiKeyAuthenticationHandler> logger2, TimeTaskServiceSettings taskServiceSettings, FeatureFlagsAuthorizationSettings featureFlagsAuthorizationSettings) : base(options, logger, encoder, clock)
    //    {
    //        _logger = logger2;
    //        _taskServiceSettings = taskServiceSettings;
    //        _featureFlagsAuthorizationSettings = featureFlagsAuthorizationSettings;
    //    }

    //    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    //    {
    //        if (Request.Headers.TryGetValue(ApiKeyHeader, out var apiHeaderValues))
    //        {
    //            if (TimeTaskServiceAuhorization.Authorize(apiHeaderValues, _taskServiceSettings, _logger))
    //            {
    //                var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Sid, apiHeaderValues[0]), new Claim(ClaimTypes.Role, "TimeTaskService"), });
    //                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    //                var authTicket = new AuthenticationTicket(claimsPrincipal, ApiKeyAuthenticationOptions.Scheme);
    //                return Task.FromResult(AuthenticateResult.Success(authTicket));
    //            }

    //            if (FeatureFlagsAuthorizationService.Authorize(apiHeaderValues, _featureFlagsAuthorizationSettings,
    //                _logger))
    //            {
    //                var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Sid, apiHeaderValues[0]), new Claim(ClaimTypes.Role, "FeatureFlagsManagment"), });
    //                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    //                var authTicket = new AuthenticationTicket(claimsPrincipal, ApiKeyAuthenticationOptions.Scheme);
    //                return Task.FromResult(AuthenticateResult.Success(authTicket));
    //            }

    //            return Task.FromResult(AuthenticateResult.NoResult());
    //        }
    //        else
    //        {
    //            return Task.FromResult(AuthenticateResult.NoResult());
    //        }
    //    }
    //}
}
