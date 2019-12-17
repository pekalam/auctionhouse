using System.Threading.Tasks;
using Core.Common;
using Core.Common.Query;
using Core.Query;
using Core.Query.Mediator;
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
        private readonly QueryMediator _mediator;

        public AuthenticationQueryController(QueryMediator mediator)
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