using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Users;

namespace Core.Command.Commands.BuyCredits
{
    [AuthorizationRequired]
    public class BuyCreditsCommand : CommandBase
    {
        [Required]
        public decimal Ammount { get; }

        [SignedInUser] public UserId SignedInUser { get; set; }

        public BuyCreditsCommand(decimal ammount)
        {
            Ammount = ammount;
        }
    }
}