﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Query.Queries.User.UserAuctions;
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
        private readonly IMediator _mediator;

        public UserQueryController(IMediator mediator)
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
    }
}