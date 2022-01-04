using Common.Application.Commands;
using System.ComponentModel.DataAnnotations;

namespace Core.Command.Commands.RequestResetPassword
{
    public class RequestResetPasswordCommand : ICommand
    {
        [Required][EmailAddress] public string Email { get; }

        public RequestResetPasswordCommand(string email)
        {
            Email = email;
        }
    }
}