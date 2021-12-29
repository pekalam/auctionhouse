using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Command;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : CommandHandlerBase<ChangePasswordCommand>
    {
        private ILogger<ChangePasswordCommandHandler> _logger;
        private IUserAuthenticationDataRepository _authenticationDataRepository;

        public ChangePasswordCommandHandler(ILogger<ChangePasswordCommandHandler> logger, IUserAuthenticationDataRepository authenticationDataRepository) : base(logger)
        {
            _logger = logger;
            _authenticationDataRepository = authenticationDataRepository;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<ChangePasswordCommand> request, CancellationToken cancellationToken)
        {
            var userAuthData = _authenticationDataRepository.FindUserAuthById(request.Command.SignedInUser);
            if (userAuthData == null)
            {
                throw new UserNotFoundException($"Cannot find {request.Command.SignedInUser} user");
            }

            userAuthData.Password = request.Command.NewPassword;

            _authenticationDataRepository.UpdateUserAuth(userAuthData);

            _logger.LogDebug("User {user} has changed password", request.Command.SignedInUser);
            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED);
            return Task.FromResult(response);
        }
    }
}