using Auctionhouse.Command.Dto;
using Auctions.Application.Commands.CreateAuction;
using AutoMapper;
using Common.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auctionhouse.Command.Controllers
{
    [ApiController]
    [Route("api")]
    //[FeatureGate("Auctionhouse_AuctionCommands")]
    public class AuctionCommandController : ControllerBase
    {
        private readonly ImmediateCommandMediator _immediateCommandMediator;
        private readonly IMapper _mapper;

        public AuctionCommandController(ImmediateCommandMediator immediateCommandMediator,
            IMapper mapper)
        {
            _immediateCommandMediator = immediateCommandMediator;
            _mapper = mapper;
        }

        [Authorize(Roles = "User"), HttpPost("createAuction")]
        public async Task<ActionResult<RequestStatusDto>> CreateAuction([FromBody] CreateAuctionCommandDto commandDto)
        {
            var cmd = _mapper.Map<CreateAuctionCommandDto, CreateAuctionCommand>(commandDto);
            var status = await _immediateCommandMediator.Send(cmd);

            return this.StatusResponse(status, _mapper.Map<RequestStatusDto>(status));
        }
    }
}