using Common.Application.Commands;
using System.ComponentModel.DataAnnotations;
using Users.Domain.Auth;

namespace Core.Command.Commands.CheckResetCode
{
    public class CheckResetCodeCommand : ICommand
    {
        [Required] public ResetCode ResetCode { get; }
        [Required] public string Email { get; }

        public CheckResetCodeCommand(ResetCode resetCode, string email)
        {
            ResetCode = resetCode;
            Email = email;
        }
    }
}