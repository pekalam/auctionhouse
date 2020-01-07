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
using Core.Command.Commands.BuyNow;
using Core.Command.Commands.EndAuction;
using Core.Command.CreateAuction;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Products;
using Core.Common.EventBus;
using Infrastructure.Services.SchedulerService;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Web.Dto.Commands;
using Web.Utils;

namespace Web.Api
{
    [ApiController]
    [Route("api")]
    [FeatureGate("Auctionhouse_AuctionCommands")]
    public class AuctionCommandController : ControllerBase
    {
        private readonly WSQueuedCommandMediator _wsCommandMediator;
        private readonly HTTPQueuedCommandMediator _httpQueuedCommandMediator;
        private readonly ImmediateCommandMediator _immediateCommandMediator;
        private readonly IMapper _mapper;

        public AuctionCommandController(WSQueuedCommandMediator wsCommandMediator, HTTPQueuedCommandMediator httpQueuedCommandMediator, ImmediateCommandMediator immediateCommandMediator, IMapper mapper)
        {
            _wsCommandMediator = wsCommandMediator;
            _httpQueuedCommandMediator = httpQueuedCommandMediator;
            _immediateCommandMediator = immediateCommandMediator;
            _mapper = mapper;
        }

        [Authorize(Roles = "User"), HttpPost("bid")]
        public async Task<ActionResult<RequestStatusDto>> Bid([FromBody] BidCommandDto commandDto)
        {
            var cmd = _mapper.MapDto<BidCommandDto, BidCommand>(commandDto);
            var response = await _wsCommandMediator.Send(cmd);

            return this.StatusResponse(response);
        }

        [Authorize(Roles = "User"), HttpPost("createAuction")]
        public async Task<ActionResult<RequestStatusDto>> CreateAuction([FromBody] CreateAuctionCommandDto commandDto)
        {
            var cmd = _mapper.MapDto<CreateAuctionCommandDto, CreateAuctionCommand>(commandDto);
            var response =await _httpQueuedCommandMediator.Send(cmd);

            return this.StatusResponse(response);
        }

        [HttpPost("endAuction"), Authorize(AuthenticationSchemes = "X-API-Key", Roles = "TimeTaskService")]
        public async Task<ActionResult<RequestStatusDto>> EndAuction([FromBody] TimeTaskRequest<AuctionEndTimeTaskValues> commandDto)
        {
            var cmd = new ScheduledTaskDispatcher().GetCommandFromTask(commandDto);
            var response =await _immediateCommandMediator.Send(cmd);

            return this.StatusResponse(response);
        }

        [Authorize(Roles = "User"), HttpPost("startCreateSession")]
        public async Task<ActionResult<RequestStatusDto>> StartCreateSession()
        {
            var cmd = new StartAuctionCreateSessionCommand();
            var response =await _immediateCommandMediator.Send(cmd);

            return this.StatusResponse(response);
        }

        [Authorize(Roles = "User"), HttpPost("removeAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> RemoveAuctionImage([FromQuery] RemoveImageCommandDto commandDto)
        {
            var cmd = new RemoveImageCommand(commandDto.ImgNum);
            var response =await _httpQueuedCommandMediator.Send(cmd);

            return this.StatusResponse(response);
        }

        [Authorize(Roles = "User"), HttpPost("addAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> AddAuctionImage([FromForm] AddAuctionImageCommandDto commandDto)
        {
            var imgRepresentation = ImageRepresentationUtil.GetImageRepresentationFromFormFile(commandDto.Img);
            var cmd = new AddAuctionImageCommand(imgRepresentation, commandDto.ImgNum);
            var response =await _httpQueuedCommandMediator.Send(cmd);

            return this.StatusResponse(response);
        }


        [Authorize(Roles = "User"), HttpPost("buyNow")]
        public async Task<ActionResult<RequestStatusDto>> BuyNow([FromBody] BuyNowCommandDto commandDto)
        {
            var cmd = _mapper.MapDto<BuyNowCommandDto, BuyNowCommand>(commandDto);
            var response =await _httpQueuedCommandMediator.Send(cmd);

            return this.StatusResponse(response);
        }
    }
}