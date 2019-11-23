using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.SignIn;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Users;
using Core.Common.Exceptions.Command;
using Microsoft.Extensions.Logging;

namespace Core.Command.ChangePassword
{
    [AuthorizationRequired]
    public class ChangePasswordCommand : ICommand
    {
        [Required]
        public string NewPassword { get; set; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }
    }

    public class ChangePasswordCommandHandler : CommandHandlerBase<ChangePasswordCommand>
    {
        private ILogger<ChangePasswordCommandHandler> _logger;
        private IUserAuthenticationDataRepository _authenticationDataRepository;

        public ChangePasswordCommandHandler(ILogger<ChangePasswordCommandHandler> logger, IUserAuthenticationDataRepository authenticationDataRepository) : base(logger)
        {
            _logger = logger;
            _authenticationDataRepository = authenticationDataRepository;
        }

        protected override Task<RequestStatus> HandleCommand(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userAuthData = _authenticationDataRepository.FindUserAuthById(request.SignedInUser.UserId);
            if (userAuthData == null)
            {
                _logger.LogError($"Cannot find {request.SignedInUser.UserId} user");
                throw new UserNotFoundException($"Cannot find {request.SignedInUser.UserId} user");
            }

            userAuthData.Password = request.NewPassword;

            _authenticationDataRepository.SaveUserAuth(userAuthData);

            var response = new RequestStatus(Status.COMPLETED);
            return Task.FromResult(response);
        }
    }
}
