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
                HttpContext.Response.Cookies.Append(DemoModeMiddleware.DemoModeDisabledKey, "true");

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

            var demoModeDisabled = context.Session.GetString(DemoModeDisabledKey) == "true";
            if (context.Request.Path.Value != "/api/c/demoCode" && !demoModeDisabled)
            {
                context.Response.Cookies.Append(DemoModeDisabledKey, "false");
                context.Response.StatusCode = 412;
                return;
            }
            else if(demoModeDisabled)
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
