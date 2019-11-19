using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Command.AddOrReplaceAuctionImage;
using Core.Common;
using Core.Common.Command;
using Core.Common.EventBus;
using MediatR;
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

        public UserCommandController(CommandMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("userAddAuctionImage")]
        public async Task<ActionResult<CommandResponseDto>> UserAddAuctionImage([FromForm] UserAddAuctionImageCommandDto commandDto)
        {
            var imgRepresentation = ImageRepresentationUtil.GetImageRepresentationFromFormFile(commandDto.Img);

            var cmd = new UserAddAuctionImageCommand(Guid.Parse(commandDto.AuctionId), imgRepresentation, commandDto.CorrelationId);
            var response = (CommandResponseDto)await _mediator.Send(cmd);

            return Ok(response);
        }
    }
}
