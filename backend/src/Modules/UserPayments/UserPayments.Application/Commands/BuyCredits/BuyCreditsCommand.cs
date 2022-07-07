using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using System.ComponentModel.DataAnnotations;


namespace Core.Command.Commands.BuyCredits
{
    [AuthorizationRequired]
    public class BuyCreditsCommand : ICommand
    {
        [Required]
        public decimal Ammount { get; }

        [SignedInUser] 
        public Guid SignedInUser { get; set; }

        public BuyCreditsCommand(decimal ammount)
        {
            Ammount = ammount;
        }
    }
}