using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
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
            var authData = _userAuthenticationDataRepository.FindUserAuth(command.UserName);
            if (authData != null)
            {
                if (authData.Password.Equals(command.Password))
                {
                    var userIdentity = new UserIdentity() { UserId = authData.UserId, UserName = authData.UserName };
                    var response = RequestStatus.CreateFromCommandContext(command.CommandContext, Status.COMPLETED, new Dictionary<string, object>()
                    {
                        {"UserIdentity", userIdentity}
                    });
                    return Task.FromResult(response);
                }
                else
                {
                    throw new InvalidPasswordException("Invalid password");
                }
            }

            throw new UserNotFoundException($"Cannot find user {command.UserName}");
        }
    }
}
