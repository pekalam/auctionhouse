using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Command;
using Core.Command.Bid;
using Core.Command.Commands.AuctionCreateSession.AddAuctionImage;
using Core.Command.Commands.AuctionCreateSession.RemoveImage;
using Core.Command.Commands.AuctionCreateSession.StartAuctionCreateSession;
using Core.Command.Commands.EndAuction;
using Core.Command.CreateAuction;
using Core.Command.Mediator;
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
        private readonly QueuedCommandMediator _mediator;
        private readonly ImmediateCommandMediator _immediateCommandMediator;
        private readonly IMapper _mapper;

        public AuctionCommandController(QueuedCommandMediator mediator, ImmediateCommandMediator immediateCommandMediator, IMapper mapper)
        {
            _mediator = mediator;
            _immediateCommandMediator = immediateCommandMediator;
            _mapper = mapper;
        }


        [Authorize(Roles = "User"), HttpPost("bid")]
        public async Task<ActionResult<RequestStatusDto>> Bid([FromBody] BidCommandDto commandDto)
        {
            var guid = Guid.Parse(commandDto.AuctionId);
            var response = (RequestStatusDto) await _mediator.Send(new BidCommand(guid, commandDto.Price));

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("createAuction")]
        public async Task<ActionResult<RequestStatusDto>> CreateAuction([FromBody] CreateAuctionCommandDto commandDto)
        {
            var command = _mapper.Map<CreateAuctionCommand>(commandDto);
            var response = (RequestStatusDto) await _immediateCommandMediator.Send(command);

            return Ok(response);
        }

        [HttpPost("endAuction"), Authorize(AuthenticationSchemes = "X-API-Key", Roles = "TimeTaskService")]
        public async Task<ActionResult<RequestStatusDto>> EndAuction([FromBody] EndAuctionCommand command)
        {
            var response = (RequestStatusDto) await _immediateCommandMediator.Send(command);

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("startCreateSession")]
        public async Task<ActionResult<RequestStatusDto>> StartCreateSession()
        {
            var command = new StartAuctionCreateSessionCommand();
            var response = (RequestStatusDto) await _immediateCommandMediator.Send(command);

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("removeAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> RemoveAuctionImage([FromQuery] RemoveImageCommandDto commandDto)
        {
            var command = new RemoveImageCommand(commandDto.ImgNum);
            var response = (RequestStatusDto) await _immediateCommandMediator.Send(command);

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("addAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> AddAuctionImage([FromForm] AddAuctionImageCommandDto commandDto)
        {
            var imgRepresentation = ImageRepresentationUtil.GetImageRepresentationFromFormFile(commandDto.Img);
            var command = new AddAuctionImageCommand(imgRepresentation, commandDto.ImgNum);
            var response = (RequestStatusDto) await _immediateCommandMediator.Send(command);

            return Ok(response);
        }
    }
}