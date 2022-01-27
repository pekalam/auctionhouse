using Common.Application.Mediator;
using Core.Command.Commands.EndAuction;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuartzTimeTaskService.AuctionEndScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Adapter.QuartzTimeTaskService.AuctionEndScheduler
{
    public class EndAuctionDto
    {
        public Guid Id { get; set; }
        public Guid AuctionId { get; set; }
    }

    [ApiController]
    [Route("api/c")]
    public class AuctionEndSchedulerEndpoint : ControllerBase
    {
        private readonly ImmediateCommandQueryMediator _mediator;

        public AuctionEndSchedulerEndpoint(ImmediateCommandQueryMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("endAuction"), Authorize(AuthenticationSchemes = ApiKeyAuthenticationOptions.Scheme, Roles = "TimeTaskService")]
        public async Task<IActionResult> EndAuction([FromBody] TimeTaskRequest<AuctionEndTimeTaskValues> request)
        {
            var cmd = new EndAuctionCommand(request.Values.AuctionId);
            await _mediator.Send(cmd);

            return Ok();
        }
    }

    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string Scheme = "X-API-Key";
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private const string ApiKeyHeader = ApiKeyAuthenticationOptions.Scheme;
        private ILogger<ApiKeyAuthenticationHandler> _logger;
        private readonly TimeTaskServiceSettings _taskServiceSettings;

        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ILogger<ApiKeyAuthenticationHandler> logger2, TimeTaskServiceSettings taskServiceSettings) : base(options, logger, encoder, clock)
        {
            _logger = logger2;
            _taskServiceSettings = taskServiceSettings;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.TryGetValue(ApiKeyHeader, out var apiHeaderValues))
            {
                if (TimeTaskServiceAuhorization.Authorize(apiHeaderValues, _taskServiceSettings, _logger))
                {
                    var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Sid, apiHeaderValues[0]), new Claim(ClaimTypes.Role, "TimeTaskService"), });
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    var authTicket = new AuthenticationTicket(claimsPrincipal, ApiKeyAuthenticationOptions.Scheme);
                    return Task.FromResult(AuthenticateResult.Success(authTicket));
                }

                //if (FeatureFlagsAuthorizationService.Authorize(apiHeaderValues, _featureFlagsAuthorizationSettings,
                //    _logger))
                {
                    var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Sid, apiHeaderValues[0]), new Claim(ClaimTypes.Role, "FeatureFlagsManagment"), });
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    var authTicket = new AuthenticationTicket(claimsPrincipal, ApiKeyAuthenticationOptions.Scheme);
                    return Task.FromResult(AuthenticateResult.Success(authTicket));
                }

                //return Task.FromResult(AuthenticateResult.NoResult());
            }
            else
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }
        }
    }
}
