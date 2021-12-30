using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Commands.RequestResetPassword;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Command;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.ResetPassword
{
    public class RequestResetPasswordCommandHandler : CommandHandlerBase<RequestResetPasswordCommand>
    {
        private ILogger<RequestResetPasswordCommandHandler> _logger;
        private IResetPasswordCodeRepository _resetPasswordCodeRepository;
        private IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private IResetLinkSenderService _linkSenderService;


        public RequestResetPasswordCommandHandler(ILogger<RequestResetPasswordCommandHandler> logger,
            IResetPasswordCodeRepository resetPasswordCodeRepository,
            IUserAuthenticationDataRepository userAuthenticationDataRepository,
            IResetLinkSenderService linkSenderService) : base(logger)
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

        protected override Task<RequestStatus> HandleCommand(AppCommand<RequestResetPasswordCommand> request,
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