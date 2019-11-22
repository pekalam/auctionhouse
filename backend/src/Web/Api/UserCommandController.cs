using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Command.AddOrReplaceAuctionImage;
using Core.Command.RemoveAuctionImage;
using Core.Command.ReplaceAuctionImage;
using Core.Command.UpdateAuction;
using Core.Common;
using Core.Common.Command;
using Core.Common.EventBus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.Commands;
using Web.Utils;

namespace Web.Api
{
    [Route("api")]
    [ApiController]
    public class UserCommandController : Controller
    {
        private readonly CommandMediator _mediator;
        private readonly IMapper _mapper;

        public UserCommandController(CommandMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }



        [Authorize(Roles = "User"), HttpPost("userAddAuctionImage")]
        public async Task<ActionResult<CommandResponseDto>> UserAddAuctionImage([FromForm] UserAddAuctionImageCommandDto commandDto)
        {
            var imgRepresentation = ImageRepresentationUtil.GetImageRepresentationFromFormFile(commandDto.Img);

            var cmd = new UserAddAuctionImageCommand(Guid.Parse(commandDto.AuctionId), imgRepresentation, commandDto.CorrelationId);
            var response = (CommandResponseDto)await _mediator.Send(cmd);

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("userReplaceAuctionImage")]
        public async Task<ActionResult<CommandResponseDto>> UserReplaceAuctionImage([FromForm] UserReplaceAuctionImageCommandDto commandDto)
        {
            var imgRepresentation = ImageRepresentationUtil.GetImageRepresentationFromFormFile(commandDto.Img);

            var cmd = new UserReplaceAuctionImageCommand(Guid.Parse(commandDto.AuctionId), imgRepresentation, commandDto.ImgNum, commandDto.CorrelationId);
            var response = (CommandResponseDto)await _mediator.Send(cmd);

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("userRemoveAuctionImage")]
        public async Task<ActionResult<CommandResponseDto>> UserRemoveAuctionImage([FromForm] UserRemoveAuctionImageCommandDto commandDto)
        {
            var cmd = new UserRemoveAuctionImageCommand(Guid.Parse(commandDto.AuctionId), commandDto.ImgNum, commandDto.CorrelationId);
            var response = (CommandResponseDto)await _mediator.Send(cmd);

            return Ok(response);
        }

        [Authorize(Roles = "User"), HttpPost("userUpdateAuction")]
        public async Task<ActionResult<CommandResponseDto>> UserUpdateAuction(
            [FromBody] UpdateAuctionCommandDto commandDto)
        {
            var cmd = _mapper.Map<UpdateAuctionCommand>(commandDto);
            var response = (CommandResponseDto)await _mediator.Send(cmd);

            return Ok(response);
        }
    }
}
