using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Core.Common;
using Core.Common.Query;
using Core.Query;
using Core.Query.Mediator;
using Core.Query.Queries.User.UserAuctions;
using Core.Query.Queries.User.UserBids;
using Core.Query.Queries.User.UserData;
using Core.Query.Queries.User.UserWonAuctions;
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
        private readonly IMapper _mapper;

        public UserQueryController(QueryMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        

        [HttpGet("userAuctions")]
        public async Task<ActionResult<UserAuctionsQueryResult>> UserAuctions(
            [FromQuery] UserAuctionsQueryDto queryDto)
        {
            var cmd = _mapper.MapDto<UserAuctionsQueryDto, UserAuctionsQuery>(queryDto);
            var auctions = await _mediator.Send(cmd);
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
        public async Task<ActionResult<UserBoughtAuctionQueryResult>> UserBoughtAuctions(
            [FromQuery] UserBoughtAuctionsQueryDto dto)
        {
            var cmd = new UserBoughtAuctionsQuery(dto.Page);
            var userWonAuctions = await _mediator.Send(cmd);
            return Ok(userWonAuctions);
        }

        [HttpGet("userWonAuctions")]
        public async Task<ActionResult<UserWonAuctionQueryResult>> UserWonAuctions(
            [FromQuery] UserWonAuctionsQueryDto dto)
        {
            var cmd = new UserWonAuctionsQuery(dto.Page);
            var userWonAuctions = await _mediator.Send(cmd);
            return Ok(userWonAuctions);
        }
    }
}