using System.ComponentModel.DataAnnotations;
using Core.Common;
using Core.Common.Command;
using Core.Common.EventBus;
using MediatR;

namespace Core.Command.SignUp
{
    public class SignUpCommand : ICommand
    {
        [Required]
        public string UserName { get; }
        [Required]
        public string Password { get; }

        public SignUpCommand(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
