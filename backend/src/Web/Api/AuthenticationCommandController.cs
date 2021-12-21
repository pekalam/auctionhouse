using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Core.Command;
using Core.Command.Commands.ChangePassword;
using Core.Command.Commands.CheckResetCode;
using Core.Command.Commands.RequestResetPassword;
using Core.Command.Commands.ResetPassword;
using Core.Command.Commands.SignIn;
using Core.Command.Mediator;
using Core.Command.SignUp;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Web.Auth;
using Web.Dto;
using Web.Dto.Commands;

namespace Web.Api
{
    [ApiController]
    [Route("api")]
    [FeatureGate("Auctionhouse_AuthenticationCommands")]
    public class AuthenticationCommandController : Controller
    {
        private readonly HTTPQueuedCommandMediator _mediator;
        private readonly ImmediateCommandMediator _immediateCommandMediator;
        private readonly JwtService _jwtService;
        private readonly IMapper _mapper;

        public AuthenticationCommandController(HTTPQueuedCommandMediator mediator, ImmediateCommandMediator immediateCommandMediator, JwtService jwtService, IMapper mapper)
        {
            _mediator = mediator;
            _immediateCommandMediator = immediateCommandMediator;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<RequestStatusDto>> SignUp([FromBody] SignUpCommandDto signUpCommandDto)
        {
            var cmd = _mapper.MapDto<SignUpCommandDto, SignUpCommand>(signUpCommandDto);
            var response = await _mediator.Send(cmd);
            return this.StatusResponse(response);
        }

        [HttpPost("signin")]
        public async Task<ActionResult<string>> SignIn([FromBody] SignInCommandDto signInCommandDto)
        {
            var cmd = _mapper.MapDto<SignInCommandDto, SignInCommand>(signInCommandDto);
            var response = await _immediateCommandMediator.Send(cmd);
            if (response.Status == Status.COMPLETED)
            {
                var userId = (UserId)response.ExtraData["UserId"];
                var username = (Username)response.ExtraData["Username"];
                var token = _jwtService.IssueToken(userId, username);

                return Ok(token);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPost("changePassword")]
        public async Task<ActionResult<RequestStatusDto>> ChangePassword([FromBody] ChangePasswordCommandDto commandDto)
        {
            var cmd = _mapper.MapDto<ChangePasswordCommandDto, ChangePasswordCommand>(commandDto);
            var result = await _mediator.Send(cmd);
            return this.StatusResponse(result);
        }

        [HttpPost("resetPassword")]
        public async Task<ActionResult<RequestStatusDto>> ResetPassword([FromBody] ResetPasswordCommandDto commandDto)
        {
            var cmd = _mapper.MapDto<ResetPasswordCommandDto, ResetPasswordCommand>(commandDto);
            var result = await _mediator.Send(cmd);

            return this.StatusResponse(result);
        }

        [HttpPost("requestResetPassword")]
        public async Task<ActionResult<RequestStatusDto>> RequestResetPassword(
            [FromBody] RequestResetPasswordCommandDto commandDto)
        {
            var cmd = _mapper.MapDto<RequestResetPasswordCommandDto, RequestResetPasswordCommand>(commandDto);
            var result = await _mediator.Send(cmd);

            return this.StatusResponse(result);
        }

        [HttpPost("checkResetCode")]
        public async Task<ActionResult<RequestStatusDto>> CheckResetCode([FromBody] CheckResetCodeCommandDto commandDto)
        {
            var cmd = _mapper.MapDto<CheckResetCodeCommandDto, CheckResetCodeCommand>(commandDto);
            var result = await _mediator.Send(cmd);

            return this.StatusResponse(result);
        }
    }
}