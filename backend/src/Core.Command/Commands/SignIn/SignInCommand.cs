using System.ComponentModel.DataAnnotations;
using Core.Common.Command;

namespace Core.Command.Commands.SignIn
{
    public class SignInCommand : ICommand
    {
        [Required]
        public string UserName { get; }
        [Required]
        public string Password { get; }

        public SignInCommand(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
