using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Core.Common.Command;

namespace Core.Command.Commands.RequestResetPassword
{
    public class RequestResetPasswordCommand : CommandBase
    {
        [Required] [EmailAddress] public string Email { get; }

        public RequestResetPasswordCommand(string email)
        {
            Email = email;
        }
    }
}