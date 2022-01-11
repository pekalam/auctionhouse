using Auctionhouse.Query.Queries;
using Common.Application.Mediator;
using Microsoft.AspNetCore.Mvc;
using ReadModel.Core.Queries.Auth.CheckUsername;

namespace Auctionhouse.Query.Controllers
{
    [ApiController]
    [Route("api/q")]
    public class UserQueryController : ControllerBase
    {
        private ImmediateCommandQueryMediator _mediator;

        public UserQueryController(ImmediateCommandQueryMediator mediator)
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
