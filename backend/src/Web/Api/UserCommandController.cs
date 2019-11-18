using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Command.AddOrReplaceAuctionImage;
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
        private readonly IMediator _mediator;

        public UserCommandController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("userAddAuctionImage")]
        public async Task<ActionResult> UserAddAuctionImage([FromForm] UserAddAuctionImageCommandDto commandDto)
        {
            var correlationId = new CorrelationId(commandDto.CorrelationId);
            var imgRepresentation = ImageRepresentationUtil.GetImageRepresentationFromFormFile(commandDto.Img);
            var cmd = new UserAddAuctionImageCommand(Guid.Parse(commandDto.AuctionId), imgRepresentation, correlationId);
            await _mediator.Send(cmd);
            return Ok();
        }
    }
}
