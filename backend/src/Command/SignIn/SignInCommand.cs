using Core.Common.Domain.Users;
using Core.Common.Interfaces;
using MediatR;

namespace Core.Command.SignIn
{
    public class SignInCommand : IRequest<UserIdentity>, ICommand
    {
        public string UserName { get; }
        public string Password { get; }

        public SignInCommand(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
