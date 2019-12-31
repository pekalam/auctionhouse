using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Command.Exceptions;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Users;

namespace Core.Command.Commands.BuyNow
{
    [AuthorizationRequired]
    public class BuyNowCommand : ICommand
    {
        public Guid AuctionId { get; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }

        public BuyNowCommand(Guid auctionId)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
        }
    }
}
