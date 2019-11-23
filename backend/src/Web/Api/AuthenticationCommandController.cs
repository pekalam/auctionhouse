using System.Security.Claims;
using System.Threading.Tasks;
using Core.Command.ChangePassword;
using Core.Command.SignIn;
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
        private readonly CommandMediator _mediator;
        private readonly JwtService _jwtService;

        public AuthenticationCommandController(CommandMediator mediator, JwtService jwtService)
        {
            _mediator = mediator;
            _jwtService = jwtService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<RequestStatus>> SignUp([FromBody] SignUpCommandDto signUpCommandDto)
        {
            var signUpCommand = new SignUpCommand(signUpCommandDto.Username, signUpCommandDto.Password);
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
    }
}