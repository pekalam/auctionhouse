using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Command;
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

        protected override Task<RequestStatus> HandleCommand(AppCommand<SignInCommand> request, CancellationToken cancellationToken)
        {
            var authData = _userAuthenticationDataRepository.FindUserAuth(request.Command.Username);
            if (authData != null)
            {
                if (authData.Password.Equals(request.Command.Password))
                {
                    var response = RequestStatus.CreatePending(request.CommandContext);
                    response.SetExtraData(new Dictionary<string, object>()
                    {
                        {"UserId", authData.UserId},
                        {"Username", authData.UserName}
                    });
                    response.MarkAsCompleted();
                    return Task.FromResult(response);
                }
                else
                {
                    throw new InvalidPasswordException("Invalid password");
                }
            }

            throw new UserNotFoundException($"Cannot find user {request.Command.Username}");
        }
    }
}
