using System.Collections.Generic;
using System.Text;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Users;

namespace Core.Command.Commands.BuyCredits
{
    [AuthorizationRequired]
    public class BuyCreditsCommand : ICommand
    {
        [SignedInUser] public UserIdentity SignedInUser { get; set; }
    }
}