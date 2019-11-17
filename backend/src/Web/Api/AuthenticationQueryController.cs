using System.Threading.Tasks;
using Core.Query.Queries.Auth.CheckUsername;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Auth;
using Web.Dto.Queries;

namespace Web.Api
{
    [ApiController]
    [Route("api")]
    public class AuthenticationQueryController : Controller
    {
        private readonly IMediator _mediator;

        public AuthenticationQueryController(IMediator mediator, JwtService jwtService)
        {
            _mediator = mediator;
        }

        [HttpGet("checkUsername")]
        public async Task<ActionResult<CheckUsernameQueryResult>> CheckUsername([FromQuery] CheckUsernameQueryDto queryDto)
        {
            var query = new CheckUsernameQuery(queryDto.Username);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}