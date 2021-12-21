using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.SignIn
{
    public class SignInCommandHandler : CommandHandlerBase<SignInCommand>
    {
        private readonly IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private readonly ILogger<SignInCommandHandler> _logger;

        public SignInCommandHandler(IUserAuthenticationDataRepository userAuthenticationDataRepository, ILogger<SignInCommandHandler> logger)
        :base(logger)
        {
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(SignInCommand command, CancellationToken cancellationToken)
        {
            var authData = _userAuthenticationDataRepository.FindUserAuth(command.Username);
            if (authData != null)
            {
                if (authData.Password.Equals(command.Password))
                {
                    var response = RequestStatus.CreateFromCommandContext(command.CommandContext, Status.COMPLETED, new Dictionary<string, object>()
                    {
                        {"UserId", authData.UserId},
                        {"Username", authData.UserName}
                    });
                    return Task.FromResult(response);
                }
                else
                {
                    throw new InvalidPasswordException("Invalid password");
                }
            }

            throw new UserNotFoundException($"Cannot find user {command.Username}");
        }
    }
}
