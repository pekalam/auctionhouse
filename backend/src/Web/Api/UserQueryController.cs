using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Query;
using Core.Query;
using Core.Query.Mediator;
using Core.Query.Queries.User.UserAuctions;
using Core.Query.Queries.User.UserBids;
using Core.Query.Queries.User.UserData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.Queries;

namespace Web.Api
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class UserQueryController : Controller
    {
        private readonly QueryMediator _mediator;

        public UserQueryController(QueryMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("userAuctions")]
        public async Task<ActionResult<UserAuctionsQueryResult>> UserAuctions(
            [FromQuery] UserAuctionsQueryDto queryDto)
        {
            var auctions = await _mediator.Send(new UserAuctionsQuery() { Page = queryDto.Page });
            return Ok(auctions);
        }

        [HttpGet("userData")]
        public async Task<ActionResult<UserDataQueryResult>> UserData()
        {
            var userData = await _mediator.Send(new UserDataQuery());
            return Ok(userData);
        }

        [HttpGet("userBids")]
        public async Task<ActionResult<UserBidsQueryResult>> UserBids()
        {
            var cmd = new UserBidsQuery();
            var userBids = await _mediator.Send(cmd);
            return Ok(userBids);
        }

        [HttpGet("userBoughtAuctions")]
        public async Task<ActionResult<UserBoughtAuctionQueryResult>> UserWonAuctions([FromQuery] UserWonAuctionsQueryDto dto)
        {
            var cmd = new UserBoughtAuctionsQuery(dto.Page);
            var userWonAuctions = await _mediator.Send(cmd);
            return Ok(userWonAuctions);
        }
    }
}
