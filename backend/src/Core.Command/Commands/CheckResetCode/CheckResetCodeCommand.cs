using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Core.Common.Auth;
using Core.Common.Command;

namespace Core.Command.Commands.CheckResetCode
{
    public class CheckResetCodeCommand : CommandBase
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