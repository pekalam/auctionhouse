using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Auth;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.ResetPassword
{
    class ResetPasswordCommandHandler : CommandHandlerBase<ResetPasswordCommand>
    {
        private readonly IResetPasswordCodeRepository _resetPasswordCodeRepository;
        private readonly IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;

        public ResetPasswordCommandHandler(ILogger<ResetPasswordCommandHandler> logger,
            IResetPasswordCodeRepository resetPasswordCodeRepository,
            IUserAuthenticationDataRepository userAuthenticationDataRepository) : base(logger)
        {
            _resetPasswordCodeRepository = resetPasswordCodeRepository;
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
            _logger = logger;
        }

        private ResetCodeRepresentation FindResetCode(ResetPasswordCommand request)
        {
            var resetCode = _resetPasswordCodeRepository.FindResetPasswordCode(request.ResetCode, request.Email);
            if (resetCode == null)
            {
                _logger.LogDebug($"Cannot find resetCode {request.ResetCode}");
                throw new InvalidCommandException($"Cannot find resetCode {request.ResetCode}");
            }

            return resetCode;
        }

        private UserAuthenticationData FindUserAuthenticationData(ResetCodeRepresentation resetCode, ResetPasswordCommand request)
        {
            var user = _userAuthenticationDataRepository.FindUserAuthByEmail(resetCode.Email);
            if (user == null)
            {
                _logger.LogWarning($"Cannot find user with email {resetCode.Email} and with reset code {request.ResetCode.Value}");
                throw new InvalidCommandException($"Cannot find user with email {resetCode.Email} and with reset code {request.ResetCode.Value}");
            }

            return user;
        }

        protected override Task<RequestStatus> HandleCommand(ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var resetCode = FindResetCode(request);
            var user = FindUserAuthenticationData(resetCode, request);

            if (resetCode.IsExpired)
            {
                _resetPasswordCodeRepository.RemoveResetPasswordCode(resetCode.ResetCode, resetCode.Email);
                return Task.FromResult(RequestStatus.CreateFromCommandContext(request.CommandContext, Status.FAILED));
            }

            if (!resetCode.Checked)
            {
                throw new InvalidCommandException("ResetCode must be checked first");
            }

            user.Password = request.NewPassword;
            _userAuthenticationDataRepository.UpdateUserAuth(user);

            return Task.FromResult(RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED));
        }
    }
}