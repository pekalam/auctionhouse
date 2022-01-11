using Auctionhouse.Query.Queries;
using Common.Application.Mediator;
using Microsoft.AspNetCore.Mvc;
using ReadModel.Core.Queries.Auth.CheckUsername;

namespace Auctionhouse.Query.Controllers
{
    [ApiController]
    [Route("api/q")]
    public class UserQueryController_Username : ControllerBase
    {
        private ImmediateCommandQueryMediator _mediator;

        public UserQueryController_Username(ImmediateCommandQueryMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("checkUsername")]
        public async Task<ActionResult<CheckUsernameQueryResult>> CheckUsername([FromQuery] CheckUsernameQueryDto queryDto)
        {
            var query = new CheckUsernameQuery(queryDto.Username);
            var result = await _mediator.SendQuery(query);
            return Ok(result);
        }
    }
}
