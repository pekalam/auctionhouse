using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Command;
using Core.Command.Commands;
using Core.Command.Commands.AuctionCreateSession.RemoveImage;
using Core.Command.Commands.BuyCredits;
using Core.Command.Commands.CancelBid;
using Core.Command.Commands.UpdateAuction;
using Core.Command.Commands.UserAddAuctionImage;
using Core.Command.Commands.UserRemoveAuctionImage;
using Core.Command.Commands.UserReplaceAuctionImage;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using Core.Common.EventBus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Web.Dto;
using Web.Dto.Commands;
using Web.Utils;

namespace Web.Api
{
    [Route("api")]
    [ApiController]
    [FeatureGate("Auctionhouse_UserCommands")]
    public class UserCommandController : Controller
    {
        private readonly WSQueuedCommandMediator _mediator;
        private readonly IMapper _mapper;

        public UserCommandController(WSQueuedCommandMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }



        [Authorize(Roles = "User"), HttpPost("userAddAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> UserAddAuctionImage([FromForm] UserAddAuctionImageCommandDto commandDto)
        {
            var imgRepresentation = ImageRepresentationUtil.GetImageRepresentationFromFormFile(commandDto.Img);

            var cmd = new UserAddAuctionImageCommand(Guid.Parse(commandDto.AuctionId), imgRepresentation);
            var response = await _mediator.Send(cmd);

            return this.StatusResponse(response);
        }

        [Authorize(Roles = "User"), HttpPost("userReplaceAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> UserReplaceAuctionImage([FromForm] UserReplaceAuctionImageCommandDto commandDto)
        {
            var imgRepresentation = ImageRepresentationUtil.GetImageRepresentationFromFormFile(commandDto.Img);

            var cmd = new UserReplaceAuctionImageCommand(Guid.Parse(commandDto.AuctionId), imgRepresentation, commandDto.ImgNum);
            var response = await _mediator.Send(cmd);

            return this.StatusResponse(response);
        }

        [Authorize(Roles = "User"), HttpPost("userRemoveAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> UserRemoveAuctionImage([FromForm] UserRemoveAuctionImageCommandDto commandDto)
        {
            var cmd = new UserRemoveAuctionImageCommand(Guid.Parse(commandDto.AuctionId), commandDto.ImgNum);
            var response = await _mediator.Send(cmd);

            return this.StatusResponse(response);
        }

        [Authorize(Roles = "User"), HttpPost("userUpdateAuction")]
        public async Task<ActionResult<RequestStatusDto>> UserUpdateAuction(
            [FromBody] UpdateAuctionCommandDto commandDto)
        {
            var cmd = _mapper.Map<UpdateAuctionCommand>(commandDto);
            var response = await _mediator.Send(cmd);

            return this.StatusResponse(response);
        }

        [Authorize(Roles = "User"), HttpPost("cancelBid")]
        public async Task<ActionResult<RequestStatusDto>> CancelBid([FromBody] CancelBidCommandDto commandDto)
        {
            var command = _mapper.Map<CancelBidCommand>(commandDto);
            var response = await _mediator.Send(command);

            return this.StatusResponse(response);
        }

        [Authorize(Roles = "User"), HttpPost("buyCredits")]
        public async Task<ActionResult<RequestStatusDto>> BuyCredits([FromBody] BuyCreditsCommandDto commandDto)
        {
            var cmd = new BuyCreditsCommand(ammount: commandDto.Ammount);
            var response = await _mediator.Send(cmd);

            return this.StatusResponse(response);
        }
    }
}
