using Auctionhouse.Command.Adapters;
using Auctionhouse.Command.Dto;
using Auctions.Application.Commands.UpdateAuction;
using Auctions.Application.Commands.UserAddAuctionImage;
using Auctions.Application.Commands.UserRemoveAuctionImage;
using Auctions.Application.Commands.UserReplaceAuctionImage;
using AutoMapper;
using Common.Application.Mediator;
using Core.Command.Commands.CancelBid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Dto.Commands;

namespace Auctionhouse.Command.Controllers
{
    [ApiController]
    [Route("api/c")]
    public class UserCommandController : ControllerBase
    {
        private readonly CommandQueryMediator _mediator;
        private readonly IMapper _mapper;

        public UserCommandController(CommandQueryMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [Authorize(Roles = "User"), HttpPost("userAddAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> UserAddAuctionImage([FromForm] UserAddAuctionImageCommandDto commandDto)
        {
            var cmd = new UserAddAuctionImageCommand(Guid.Parse(commandDto.AuctionId), new FileStreamAccessor(commandDto.Img), commandDto.Img.FileName.GetFileExtensionOrThrow400());
            var status = await _mediator.Send(cmd);

            return this.StatusResponse(status);
        }

        [Authorize(Roles = "User"), HttpPost("userReplaceAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> UserReplaceAuctionImage([FromForm] UserReplaceAuctionImageCommandDto commandDto)
        {

            var cmd = new UserReplaceAuctionImageCommand(Guid.Parse(commandDto.AuctionId), new FileStreamAccessor(commandDto.Img), commandDto.ImgNum, commandDto.Img.FileName.GetFileExtensionOrThrow400());
            var status = await _mediator.Send(cmd);

            return this.StatusResponse(status);
        }

        [Authorize(Roles = "User"), HttpPost("userRemoveAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> UserRemoveAuctionImage([FromForm] UserRemoveAuctionImageCommandDto commandDto)
        {
            var cmd = new UserRemoveAuctionImageCommand(Guid.Parse(commandDto.AuctionId), commandDto.ImgNum);
            var status = await _mediator.Send(cmd);

            return this.StatusResponse(status);
        }

        [Authorize(Roles = "User"), HttpPost("userUpdateAuction")]
        public async Task<ActionResult<RequestStatusDto>> UserUpdateAuction([FromBody] UpdateAuctionCommandDto commandDto)
        {
            var cmd = _mapper.Map<UpdateAuctionCommand>(commandDto);
            var status = await _mediator.Send(cmd);

            return this.StatusResponse(status);
        }
    }
}
