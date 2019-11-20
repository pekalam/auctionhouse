using System.ComponentModel.DataAnnotations;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain.Users;
using MediatR;

namespace Core.Command.SignIn
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
