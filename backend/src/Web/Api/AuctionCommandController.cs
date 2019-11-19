using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Command.AuctionCreateSession.AuctionCreateSession_AddAuctionImage;
using Core.Command.AuctionCreateSession.AuctionCreateSession_RemoveImage;
using Core.Command.AuctionCreateSession.AuctionCreateSession_StartAuctionCreateSession;
using Core.Command.Bid;
using Core.Command.CreateAuction;
using Core.Command.EndAuction;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Products;
using Core.Common.EventBus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.Commands;
using Web.Utils;

namespace Web.Api
{
    [ApiController]
    [Route("api")]
    public class AuctionCommandController : ControllerBase
    {
        private readonly CommandMediator _mediator;
        private readonly IMapper _mapper;

        public AuctionCommandController(CommandMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [Authorize(Roles = "User"), HttpPost("bid")]
        public async Task<ActionResult<CommandResponseDto>> Bid([FromBody] BidCommandDto commandDto)
        {
            var guid = Guid.Parse(commandDto.AuctionId);
            var response = (CommandResponseDto) await _mediator.Send(new BidCommand(guid, commandDto.Price, commandDto.CorrelationId));

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("createAuction")]
        public async Task<ActionResult<CommandResponseDto>> CreateAuction([FromBody] CreateAuctionCommandDto commandDto)
        {
            var command = _mapper.Map<CreateAuctionCommand>(commandDto);
            var response = (CommandResponseDto) await _mediator.Send(command);

            return Ok(response);
        }

        [HttpPost("endAuction"), Authorize(AuthenticationSchemes = "X-API-Key", Roles = "TimeTaskService")]
        public async Task<ActionResult<CommandResponseDto>> EndAuction([FromBody] EndAuctionCommand command)
        {
            var response = (CommandResponseDto) await _mediator.Send(command);

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("startCreateSession")]
        public async Task<ActionResult<CommandResponseDto>> StartCreateSession([FromBody] StartAuctionCreateSessionCommandDto commandDto)
        {
            var command = new StartAuctionCreateSessionCommand(commandDto.CorrelationId);
            var response = (CommandResponseDto) await _mediator.Send(command);

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("removeAuctionImage")]
        public async Task<ActionResult<CommandResponseDto>> RemoveAuctionImage([FromQuery] RemoveImageCommandDto commandDto)
        {
            var command = new RemoveImageCommand(commandDto.ImgNum);
            var response = (CommandResponseDto) await _mediator.Send(command);

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("addAuctionImage")]
        public async Task<ActionResult<CommandResponseDto>> AddAuctionImage([FromForm] AddAuctionImageCommandDto commandDto)
        {
            var imgRepresentation = ImageRepresentationUtil.GetImageRepresentationFromFormFile(commandDto.Img);
            var command = new AddAuctionImageCommand(imgRepresentation, commandDto.CorrelationId, commandDto.ImgNum);
            var response = (CommandResponseDto) await _mediator.Send(command);

            return Ok(response);
        }
    }
}