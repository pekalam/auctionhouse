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
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;


        public AuctionCommandController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [Authorize(Roles = "User"), HttpPost("bid")]
        public async Task<ActionResult> Bid([FromBody] BidCommandDto commandDto)
        {
            var guid = Guid.Parse(commandDto.AuctionId);
            await _mediator.Send(new BidCommand(guid, commandDto.Price, new CorrelationId(commandDto.CorrelationId)));
            return Ok();
        }

        [Authorize(Roles = "User"), HttpPost("createAuction")]
        public async Task<ActionResult> CreateAuction([FromBody] CreateAuctionCommandDto commandDto)
        {
            var command = _mapper.Map<CreateAuctionCommand>(commandDto);
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPost("endAuction"), Authorize(AuthenticationSchemes = "X-API-Key", Roles = "TimeTaskService")]
        public async Task<ActionResult> EndAuction([FromBody] EndAuctionCommand command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [Authorize(Roles = "User"), HttpPost("startCreateSession")]
        public async Task<ActionResult> StartCreateSession([FromBody] StartAuctionCreateSessionCommandDto commandDto)
        {
            var correlationId = new CorrelationId(commandDto.CorrelationId);
            var command = new StartAuctionCreateSessionCommand(correlationId);
            await _mediator.Send(command);
            return Ok();
        }

        [Authorize(Roles = "User"), HttpPost("removeAuctionImage")]
        public async Task<ActionResult> RemoveAuctionImage([FromQuery] RemoveImageCommandDto commandDto)
        {
            var command = new RemoveImageCommand()
            {
                ImgNum = commandDto.ImgNum
            };
            await _mediator.Send(command);
            return Ok();
        }

        [Authorize(Roles = "User"), HttpPost("addAuctionImage")]
        public async Task<ActionResult> AddAuctionImage([FromForm] AddAuctionImageCommandDto commandDto)
        {
            var correlationId = new CorrelationId(commandDto.CorrelationId);
            var imgRepresentation = ImageRepresentationUtil.GetImageRepresentationFromFormFile(commandDto.Img);
            var command = new AddAuctionImageCommand(imgRepresentation, correlationId, commandDto.ImgNum);
            await _mediator.Send(command);
            return Ok();
        }
    }
}