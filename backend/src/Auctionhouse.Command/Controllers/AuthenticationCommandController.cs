using Auctionhouse.Command.Dto;
using AutoMapper;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Mediator;
using Common.WebAPI.Auth;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Commands.ChangePassword;
using Users.Application.Commands.CheckResetCode;
using Users.Application.Commands.RequestResetPassword;
using Users.Application.Commands.ResetPassword;
using Users.Application.Commands.SignIn;
using Users.Application.Commands.SignUp;

namespace Auctionhouse.Command.Controllers
{
    [ApiController]
    [Route("api/c")]
    //[FeatureGate("Auctionhouse_AuthenticationCommands")]
    public class AuthenticationCommandController : Controller
    {
        private readonly ImmediateCommandQueryMediator _mediator;
        private readonly JwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly Lazy<IIdTokenManager> _idTokenManager;

        public AuthenticationCommandController(ImmediateCommandQueryMediator immediateCommandMediator, JwtService jwtService, IMapper mapper, Lazy<IIdTokenManager> idTokenManager)
        {
            _mediator = immediateCommandMediator;
            _jwtService = jwtService;
            _mapper = mapper;
            _idTokenManager = idTokenManager;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<RequestStatusDto>> SignUp([FromBody] SignUpCommandDto signUpCommandDto)
        {
            var cmd = _mapper.Map<SignUpCommandDto, SignUpCommand>(signUpCommandDto);
            var status = await _mediator.Send(cmd);
            return this.StatusResponse(status);
        }

        [HttpPost("signin")]
        public async Task<ActionResult<string>> SignIn([FromBody] SignInCommandDto signInCommandDto)
        {
            var cmd = _mapper.Map<SignInCommandDto, SignInCommand>(signInCommandDto);
            var response = await _mediator.Send(cmd);
            if (response.Status == Status.COMPLETED)
            {
                var userId = (Guid)response.ExtraData!["UserId"];
                var username = (string)response.ExtraData["Username"];
                var token = _jwtService.IssueToken(userId, username);

                HttpContext.Response.Cookies.Append("IdToken", token, new()
                {
                    HttpOnly = true,
                    Secure = true,
                });

                return Ok(token);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPost("signout")]
        public async Task<IActionResult> SignOut_Async()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("IdToken") || HttpContext.Request.Cookies["IdToken"] == null)
            {
                return BadRequest();
            }
            var token = HttpContext.Request.Cookies["IdToken"]!;
            await _idTokenManager.Value.DeactivateToken(token, CancellationToken.None);

            HttpContext.Response.Cookies.Delete("IdToken");
            return Ok();
        }

        [HttpPost("changePassword")]
        public async Task<ActionResult<RequestStatusDto>> ChangePassword([FromBody] ChangePasswordCommandDto commandDto)
        {
            var cmd = _mapper.Map<ChangePasswordCommandDto, ChangePasswordCommand>(commandDto);
            var status = await _mediator.Send(cmd);
            return this.StatusResponse(status);
        }

        [HttpPost("resetPassword")]
        public async Task<ActionResult<RequestStatusDto>> ResetPassword([FromBody] ResetPasswordCommandDto commandDto)
        {
            var cmd = _mapper.Map<ResetPasswordCommandDto, ResetPasswordCommand>(commandDto);
            var status = await _mediator.Send(cmd);

            return this.StatusResponse(status);
        }

        [HttpPost("requestResetPassword")]
        public async Task<ActionResult<RequestStatusDto>> RequestResetPassword(
            [FromBody] RequestResetPasswordCommandDto commandDto)
        {
            var cmd = _mapper.Map<RequestResetPasswordCommandDto, RequestResetPasswordCommand>(commandDto);
            var status = await _mediator.Send(cmd);

            return this.StatusResponse(status);
        }

        [HttpPost("checkResetCode")]
        public async Task<ActionResult<RequestStatusDto>> CheckResetCode([FromBody] CheckResetCodeCommandDto commandDto)
        {
            var cmd = _mapper.Map<CheckResetCodeCommandDto, CheckResetCodeCommand>(commandDto);
            var status = await _mediator.Send(cmd);

            return this.StatusResponse(status);
        }
    }
}