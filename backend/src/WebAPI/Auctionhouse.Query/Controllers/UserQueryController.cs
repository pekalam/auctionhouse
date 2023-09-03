using Auctionhouse.Query.Queries;
using AutoMapper;
using Common.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadModel.Contracts.Queries.User.UserAuctions;
using ReadModel.Contracts.Queries.User.UserBids;
using ReadModel.Contracts.Queries.User.UserBoughtAuctions;
using ReadModel.Contracts.Queries.User.UserData;
using ReadModel.Contracts.Queries.User.UserWonAuctions;

namespace Auctionhouse.Query.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/q")]
    public class UserQueryController : ControllerBase
    {
        private CommandQueryMediator _mediator;
        private IMapper _mapper;

        public UserQueryController(CommandQueryMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet("userAuctions")]
        public async Task<ActionResult<UserAuctionsQueryResult>> UserAuctions(
      [FromQuery] UserAuctionsQueryDto queryDto)
        {
            var query = _mapper.Map<UserAuctionsQueryDto, UserAuctionsQuery>(queryDto);
            var auctions = await _mediator.SendQuery(query);
            return Ok(auctions);
        }

        [HttpGet("userData")]
        public async Task<ActionResult<UserDataQueryResult>> UserData()
        {
            var userData = await _mediator.SendQuery(new UserDataQuery());
            return Ok(userData);
        }

        [HttpGet("userBids")]
        public async Task<ActionResult<UserBidsQueryResult>> UserBids()
        {
            var query = new UserBidsQuery();
            var userBids = await _mediator.SendQuery(query);
            return Ok(userBids);
        }

        [HttpGet("userBoughtAuctions")]
        public async Task<ActionResult<UserBoughtAuctionQueryResult>> UserBoughtAuctions(
            [FromQuery] UserBoughtAuctionsQueryDto dto)
        {
            var query = _mapper.Map<UserBoughtAuctionsQueryDto, UserBoughtAuctionsQuery>(dto);
            var userWonAuctions = await _mediator.SendQuery(query);
            return Ok(userWonAuctions);
        }

        [HttpGet("userWonAuctions")]
        public async Task<ActionResult<UserWonAuctionQueryResult>> UserWonAuctions(
            [FromQuery] UserWonAuctionsQueryDto dto)
        {
            var query = _mapper.Map<UserWonAuctionsQueryDto, UserWonAuctionsQuery>(dto);
            var userWonAuctions = await _mediator.SendQuery(query);
            return Ok(userWonAuctions);
        }
    }
}
