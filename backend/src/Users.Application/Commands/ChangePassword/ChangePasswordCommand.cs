using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Core.Command.Commands.ChangePassword
{
    [AuthorizationRequired]
    public class ChangePasswordCommand : ICommand
    {
        [Required]
        public string NewPassword { get; set; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }
    }
}
