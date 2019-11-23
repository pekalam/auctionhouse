using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.SignIn
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
                    var response = new RequestStatus(Status.COMPLETED, new Dictionary<string, object>()
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
