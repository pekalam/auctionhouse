using System;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;
using Users.Domain.Auth;
using Users.Domain.Repositories;

namespace Users.Application.Commands.CheckResetCode
{
    public class CheckResetCodeCommandHandler : CommandHandlerBase<CheckResetCodeCommand>
    {
        private readonly IResetPasswordCodeRepository _resetPasswordCodeRepository;
        private readonly IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private readonly ILogger<CheckResetCodeCommandHandler> _logger;

        public CheckResetCodeCommandHandler(IResetPasswordCodeRepository resetPasswordCodeRepository,
            IUserAuthenticationDataRepository userAuthenticationDataRepository,
            ILogger<CheckResetCodeCommandHandler> logger, CommandHandlerBaseDependencies dependencies) 
            : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _resetPasswordCodeRepository = resetPasswordCodeRepository;
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
            _logger = logger;
        }

        private ResetCodeRepresentation FindResetPasswordCode(AppCommand<CheckResetCodeCommand> request)
        {
            var resetCode = _resetPasswordCodeRepository.FindResetPasswordCode(request.Command.ResetCode, request.Command.Email);
            if (resetCode == null)
            {
                throw new InvalidCommandException($"Cannot find resetCode {request.Command.ResetCode}");
            }

            return resetCode;
        }

        private void CheckIfUserExists(ResetCodeRepresentation resetCode)
        {
            var user = _userAuthenticationDataRepository.FindUserAuthByEmail(resetCode.Email);
            if (user == null)
            {
                throw new InvalidCommandException(
                    $"Cannot find user with email: {resetCode.Email} and with reset code {resetCode.ResetCode.Value}");
            }
        }

        protected override Task<RequestStatus> HandleCommand(
            AppCommand<CheckResetCodeCommand> request,
            IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var resetCode = FindResetPasswordCode(request);
            CheckIfUserExists(resetCode);

            if (!resetCode.IsExpired)
            {
                resetCode.MarkAsChecked();
                _resetPasswordCodeRepository.UpdateResetPasswordCode(resetCode);
            }

            var result = RequestStatus.CreatePending(request.CommandContext);
            result.SetExtraData(new Dictionary<string, object>()
                {
                    {"expired", resetCode.IsExpired}
                });
            result.MarkAsCompleted();
            _logger.LogDebug("Reset code: {@resetCode} checked", resetCode);

            return Task.FromResult(result);
        }
    }
}