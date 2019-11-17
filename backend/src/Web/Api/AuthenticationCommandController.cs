using System.Security.Claims;
using System.Threading.Tasks;
using Core.Command.SignIn;
using Core.Command.SignUp;
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
        private readonly IMediator _mediator;
        private readonly JwtService _jwtService;

        public AuthenticationCommandController(IMediator mediator, JwtService jwtService)
        {
            _mediator = mediator;
            _jwtService = jwtService;
        }


        [HttpPost("signup")]
        public async Task<ActionResult> SignUp([FromBody] SignUpCommandDto signUpCommandDto)
        {
            var signUpCommand = new SignUpCommand(signUpCommandDto.Username, signUpCommandDto.Password,
                new CorrelationId(signUpCommandDto.CorrelationId));
            await _mediator.Send(signUpCommand);
            return Ok();
        }

        [HttpPost("signin")]
        public async Task<ActionResult<string>> SignIn([FromBody] SignInCommandDto signInCommandDto)
        {
            var signInCommand = new SignInCommand(signInCommandDto.UserName, signInCommandDto.Password);
            var userIdentity = await _mediator.Send(signInCommand);
            var token = _jwtService.IssueToken(userIdentity.UserId, userIdentity.UserName);
            return Ok(token);
        }
    }
}