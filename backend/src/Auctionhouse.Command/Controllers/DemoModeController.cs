using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Auctionhouse.Command.Controllers
{
    public class DemoCodeDto
    {
        public string DemoCode { get; set; }
    }

    [ApiController]
    [Route("api/c")]
    public class DemoModeController : ControllerBase
    {
        private readonly IOptionsMonitor<DemoModeOptions> _demoModeOptions;

        public DemoModeController(IOptionsMonitor<DemoModeOptions> demoModeOptions)
        {
            _demoModeOptions = demoModeOptions;
        }

        [HttpPost("demoCode")]
        public IActionResult SubmitDemoCode([FromBody] DemoCodeDto dto)
        {
            if(dto.DemoCode == _demoModeOptions.CurrentValue.DemoCode)
            {
                HttpContext.Session.SetString(DemoModeMiddleware.DemoModeDisabledKey, "true");

                return Ok();
            }
            return Unauthorized();
        }
    }

    public class DemoModeMiddleware
    {
        public const string DemoModeDisabledKey = "demoModeDisabled";
        private readonly RequestDelegate _next;

        public DemoModeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOptionsMonitor<DemoModeOptions> options) 
        {
            if (!options.CurrentValue.Enabled)
            {
                await _next(context);
                return;
            }

            if (context.Request.Path.Value != "/api/c/demoCode" && context.Session.GetString(DemoModeDisabledKey) != "true")
            {
                context.Response.StatusCode = 401;
                return;
            }
            else
            {
                context.Response.Cookies.Append(DemoModeDisabledKey, "true");
            }

            await _next(context);
        }
    }

    public class DemoModeOptions
    {
        public bool Enabled { get; set; }

        public string DemoCode { get; set; }
    }
}
