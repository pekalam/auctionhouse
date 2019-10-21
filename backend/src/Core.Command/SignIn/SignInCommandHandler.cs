using System.Threading;
using System.Threading.Tasks;
using Core.Common.Auth;
using Core.Common.Domain.Users;
using MediatR;

namespace Core.Command.SignIn
{
    public class SignInCommandHandler : IRequestHandler<SignInCommand, UserIdentity>
    {
        private readonly IUserAuthenticationDataRepository _userAuthenticationDataRepository;

        public SignInCommandHandler(IUserAuthenticationDataRepository userAuthenticationDataRepository)
        {
            _userAuthenticationDataRepository = userAuthenticationDataRepository;
        }

        public Task<UserIdentity> Handle(SignInCommand command, CancellationToken cancellationToken)
        {
            var authData = _userAuthenticationDataRepository.FindUserAuth(command.UserName);
            if (authData != null)
            {
                if (authData.Password.Equals(command.Password))
                {
                    var userIdentity = new UserIdentity(){UserId = authData.UserId, UserName = authData.UserName};
                    return Task.FromResult(userIdentity);
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
