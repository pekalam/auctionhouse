using Common.Application.Commands;
using System.ComponentModel.DataAnnotations;
using Users.Domain.Auth;

namespace Core.Command.Commands.ResetPassword
{
    public class ResetPasswordCommand : ICommand
    {
        [Required] public string NewPassword { get; }
        [Required] public ResetCode ResetCode { get; }
        [Required] public string Email { get; }

        public ResetPasswordCommand(string newPassword, ResetCode resetCode, string email)
        {
            NewPassword = newPassword;
            ResetCode = resetCode;
            Email = email;
        }
    }
}