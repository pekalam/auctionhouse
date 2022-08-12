using System;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;
using Users.Domain.Auth;
using Users.Domain.Repositories;

namespace Users.Application.Commands.ResetPassword
{
    class ResetPasswordCommandHandler : CommandHandlerBase<ResetPasswordCommand>
    {
        private readonly IResetPasswordCodeRepository _resetPasswordCodeRepository;
        private readonly IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;

        public ResetPasswordCommandHandler(ILogger<ResetPasswordCommandHandler> logger,
            IResetPasswordCodeRepository resetPasswordCodeRepository,
            IUserAuthenticationDataRepository userAuthenticationDataRepository,
            CommandHandlerBaseDependencies dependencies) 
            : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _resetPasswordCodeRepository = resetPasswordCodeRepository;
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
            _logger = logger;
        }

        private ResetCodeRepresentation FindResetCode(AppCommand<ResetPasswordCommand> request)
        {
            var resetCode = _resetPasswordCodeRepository.FindResetPasswordCode(request.Command.ResetCode, request.Command.Email);
            if (resetCode == null)
            {
                throw new InvalidCommandException($"Cannot find resetCode {request.Command.ResetCode}");
            }

            return resetCode;
        }

        private UserAuthenticationData FindUserAuthenticationData(ResetCodeRepresentation resetCode, AppCommand<ResetPasswordCommand> request)
        {
            var user = _userAuthenticationDataRepository.FindUserAuthByEmail(resetCode.Email);
            if (user == null)
            {
                throw new InvalidCommandException($"Cannot find user with email {resetCode.Email} and with reset code {request.Command.ResetCode.Value}");
            }

            return user;
        }

        protected override Task<RequestStatus> HandleCommand(
            AppCommand<ResetPasswordCommand> request,
            IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var resetCode = FindResetCode(request);
            var user = FindUserAuthenticationData(resetCode, request);
            var requestStatus = RequestStatus.CreatePending(request.CommandContext);

            if (resetCode.IsExpired)
            {
                _resetPasswordCodeRepository.RemoveResetPasswordCode(resetCode.ResetCode, resetCode.Email);
                requestStatus.MarkAsFailed();
                return Task.FromResult(requestStatus);
            }

            if (!resetCode.Checked)
            {
                throw new InvalidCommandException("ResetCode must be checked first");
            }

            user.Password = request.Command.NewPassword;
            _userAuthenticationDataRepository.UpdateUserAuth(user);

            _logger.LogInformation("User with email {email} and reset code {@resetCode} has changed password", request.Command.Email, resetCode);
            requestStatus.MarkAsCompleted();
            return Task.FromResult(requestStatus);
        }
    }
}