using Core.Common;
using Core.Common.Command;
using Core.Common.Domain.Users;
using MediatR;

namespace Core.Command.SignIn
{
    public class SignInCommand : ICommand
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
