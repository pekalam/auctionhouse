using Auctionhouse.Command.Adapters;
using Auctionhouse.Command.Dto;
using Auctions.Application.Commands.AddAuctionImage;
using Auctions.Application.Commands.CreateAuction;
using Auctions.Application.Commands.RemoveImage;
using Auctions.Application.Commands.StartAuctionCreateSession;
using AutoMapper;
using Common.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auctionhouse.Command.Controllers
{
    [ApiController]
    [Route("api/c")]
    //[FeatureGate("Auctionhouse_AuctionCommands")]
    public class AuctionCommandController : ControllerBase
    {
        private readonly ImmediateCommandQueryMediator _immediateCommandMediator;
        private readonly IMapper _mapper;

        public AuctionCommandController(ImmediateCommandQueryMediator immediateCommandMediator,
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

        [Authorize(Roles = "User"), HttpPost("startCreateSession")]
        public async Task<ActionResult<RequestStatusDto>> StartCreateSession()
        {
            var cmd = new StartAuctionCreateSessionCommand();
            var status = await _immediateCommandMediator.Send(cmd);

            return this.StatusResponse(status, _mapper.Map<RequestStatusDto>(status));
        }

        [Authorize(Roles = "User"), HttpPost("removeAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> RemoveAuctionImage([FromQuery] RemoveImageCommandDto commandDto)
        {
            var cmd = new RemoveImageCommand(commandDto.ImgNum);
            var status = await _immediateCommandMediator.Send(cmd);

            return this.StatusResponse(status, _mapper.Map<RequestStatusDto>(status));
        }

        [Authorize(Roles = "User"), HttpPost("addAuctionImage")]
        public async Task<ActionResult<RequestStatusDto>> AddAuctionImage([FromForm] AddAuctionImageCommandDto commandDto)
        {
            var cmd = new AddAuctionImageCommand
            {
                Img = new FileStreamAccessor(commandDto.Img),
                ImgNum = commandDto.ImgNum,
                Extension = commandDto.Img.FileName.GetFileExtensionOrThrow400()
            };
            var status = await _immediateCommandMediator.Send(cmd);

            return this.StatusResponse(status, _mapper.Map<RequestStatusDto>(status));
        }
    }
}