using System.Security.Claims;
using System.Threading.Tasks;
using Core.Command;
using Core.Command.Commands.ChangePassword;
using Core.Command.Commands.CheckResetCode;
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
using Web.Auth;
using Web.Dto;
using Web.Dto.Commands;

namespace Web.Api
{
    [ApiController]
    [Route("api")]
    public class AuthenticationCommandController : Controller
    {
        private readonly ImmediateCommandMediator _mediator;
        private readonly JwtService _jwtService;

        public AuthenticationCommandController(ImmediateCommandMediator mediator, JwtService jwtService)
        {
            _mediator = mediator;
            _jwtService = jwtService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<RequestStatus>> SignUp([FromBody] SignUpCommandDto signUpCommandDto)
        {
            var signUpCommand = new SignUpCommand(signUpCommandDto.Username, signUpCommandDto.Password, signUpCommandDto.Email);
            var response = (RequestStatusDto) await _mediator.Send(signUpCommand);
            return Ok(response);
        }

        [HttpPost("signin")]
        public async Task<ActionResult<string>> SignIn([FromBody] SignInCommandDto signInCommandDto)
        {
            var signInCommand = new SignInCommand(signInCommandDto.UserName, signInCommandDto.Password);
            var response = await _mediator.Send(signInCommand);
            if (response.Status == Status.COMPLETED)
            {
                var userIdentity = (UserIdentity)response.ExtraData["UserIdentity"];
                var token = _jwtService.IssueToken(userIdentity.UserId, userIdentity.UserName);

                return Ok(token);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPost("changePassword")]
        public async Task<ActionResult<RequestStatus>> ChangePassword([FromBody] ChangePasswordCommandDto commandDto)
        {
            var cmd = new ChangePasswordCommand()
            {
                NewPassword = commandDto.NewPassword
            };
            var result = await _mediator.Send(cmd);
            return result;
        }

        [HttpPost("resetPassword")]
        public async Task<ActionResult<RequestStatusDto>> ResetPassword([FromBody] ResetPasswordCommandDto commandDto)
        {
            var cmd = new ResetPasswordCommand(commandDto.NewPassword, commandDto.ResetCode, commandDto.Email);
            var result = (RequestStatusDto) await _mediator.Send(cmd);

            return Ok(result);
        }

        [HttpPost("requestResetPassword")]
        public async Task<ActionResult<RequestStatusDto>> RequestResetPassword(
            [FromBody] RequestResetPasswordCommandDto commandDto)
        {
            var cmd = new RequestResetPasswordCommand(commandDto.Email);
            var result = (RequestStatusDto)await _mediator.Send(cmd);

            return Ok(result);
        }

        [HttpPost("checkResetCode")]
        public async Task<ActionResult<RequestStatusDto>> CheckResetCode([FromBody] CheckResetCodeCommandDto commandDto)
        {
            var cmd = new CheckResetCodeCommand(commandDto.ResetCode, commandDto.Email);
            var result = (RequestStatusDto)await _mediator.Send(cmd);

            return Ok(result);
        }
    }
}