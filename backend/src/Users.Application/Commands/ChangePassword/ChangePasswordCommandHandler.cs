using Common.Application;
using Common.Application.Commands;
using Microsoft.Extensions.Logging;
using Users.Application.Exceptions;
using Users.Domain.Repositories;

namespace Users.Application.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : CommandHandlerBase<ChangePasswordCommand>
    {
        private readonly ILogger<ChangePasswordCommandHandler> _logger;
        private readonly IUserAuthenticationDataRepository _authenticationDataRepository;

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
            var response = RequestStatus.CreatePending(request.CommandContext);
            response.MarkAsCompleted();
            return Task.FromResult(response);
        }
    }
}