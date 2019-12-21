using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Auth;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.CheckResetCode
{
    public class CheckResetCodeCommandHandler : CommandHandlerBase<CheckResetCodeCommand>
    {
        private IResetPasswordCodeRepository _resetPasswordCodeRepository;
        private IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private ILogger<CheckResetCodeCommandHandler> _logger;

        public CheckResetCodeCommandHandler(IResetPasswordCodeRepository resetPasswordCodeRepository,
            IUserAuthenticationDataRepository userAuthenticationDataRepository,
            ILogger<CheckResetCodeCommandHandler> logger) : base(logger)
        {
            _resetPasswordCodeRepository = resetPasswordCodeRepository;
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
            _logger = logger;
        }

        private ResetCodeRepresentation FindResetPasswordCode(CheckResetCodeCommand request)
        {
            var resetCode = _resetPasswordCodeRepository.FindResetPasswordCode(request.ResetCode, request.Email);
            if (resetCode == null)
            {
                _logger.LogDebug($"Cannot find resetCode {request.ResetCode}");
                throw new InvalidCommandException($"Cannot find resetCode {request.ResetCode}");
            }

            return resetCode;
        }

        private void CheckIfUserExists(ResetCodeRepresentation resetCode)
        {
            var user = _userAuthenticationDataRepository.FindUserAuthByEmail(resetCode.Email);
            if (user == null)
            {
                _logger.LogWarning($"Cannot find user with email: {resetCode.Email} and with reset code {resetCode.ResetCode.Value}");
                throw new InvalidCommandException(
                    $"Cannot find user with email: {resetCode.Email} and with reset code {resetCode.ResetCode.Value}");
            }
        }

        protected override Task<RequestStatus> HandleCommand(CheckResetCodeCommand request,
            CancellationToken cancellationToken)
        {
            var resetCode = FindResetPasswordCode(request);
            CheckIfUserExists(resetCode);

            if (!resetCode.IsExpired)
            {
                resetCode.MarkAsChecked();
                _resetPasswordCodeRepository.UpdateResetPasswordCode(resetCode);
            }

            var result = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED,
                new Dictionary<string, object>()
                {
                    {"expired", resetCode.IsExpired}
                });


            return Task.FromResult(result);
        }
    }
}