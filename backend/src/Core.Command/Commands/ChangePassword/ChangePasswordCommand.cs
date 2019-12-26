using System.ComponentModel.DataAnnotations;
using Core.Command.Commands.SignIn;
using Core.Command.Mediator;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Users;

namespace Core.Command.Commands.ChangePassword
{
    [AuthorizationRequired]
    public class ChangePasswordCommand : ICommand
    {
        [Required]
        public string NewPassword { get; set; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }
    }
}
