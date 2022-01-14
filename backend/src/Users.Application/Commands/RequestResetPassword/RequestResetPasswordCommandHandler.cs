using System;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.Logging;
using Users.Domain.Auth;
using Users.Domain.Repositories;
using Users.Domain.Services;

namespace Users.Application.Commands.RequestResetPassword
{
    public class RequestResetPasswordCommandHandler : CommandHandlerBase<RequestResetPasswordCommand>
    {
        private readonly ILogger<RequestResetPasswordCommandHandler> _logger;
        private readonly IResetPasswordCodeRepository _resetPasswordCodeRepository;
        private readonly IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private readonly IResetLinkSenderService _linkSenderService;


        public RequestResetPasswordCommandHandler(ILogger<RequestResetPasswordCommandHandler> logger,
            IResetPasswordCodeRepository resetPasswordCodeRepository,
            IUserAuthenticationDataRepository userAuthenticationDataRepository,
            IResetLinkSenderService linkSenderService, CommandHandlerBaseDependencies dependencies) 
            : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _logger = logger;
            _resetPasswordCodeRepository = resetPasswordCodeRepository;
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
            _linkSenderService = linkSenderService;
        }

        private UserAuthenticationData FindUserAuthenticationData(AppCommand<RequestResetPasswordCommand> request)
        {
            var userAuthData = _userAuthenticationDataRepository.FindUserAuthByEmail(request.Command.Email);
            if (userAuthData == null)
            {
                throw new InvalidCommandException($"Cannot find user with email: {request.Command.Email}");
            }

            return userAuthData;
        }

        protected override Task<RequestStatus> HandleCommand(
            AppCommand<RequestResetPasswordCommand> request,
            IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var userAuthData = FindUserAuthenticationData(request);

            var existingResetCode = _resetPasswordCodeRepository.CountResetCodesForEmail(userAuthData.Email);
            if (existingResetCode > 0)
            {
                _logger.LogDebug("Removing {ex} existing codes for email {email}", existingResetCode, userAuthData.Email);
                _resetPasswordCodeRepository.RemoveResetCodesByEmail(userAuthData.Email);
            }

            var resetCode = new ResetCodeRepresentation(0, "000000", DateTime.UtcNow, false, userAuthData.Email);
            resetCode = _resetPasswordCodeRepository.CreateResetPasswordCode(resetCode);

            _linkSenderService.SendResetLink(resetCode.ResetCode, userAuthData.UserName, userAuthData.Email);

            var requestStatus = RequestStatus.CreatePending(request.CommandContext);
            requestStatus.MarkAsCompleted();
            return Task.FromResult(requestStatus);
        }
    }
}