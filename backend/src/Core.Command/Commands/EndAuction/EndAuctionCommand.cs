using System;
using Core.Command.Exceptions;
using Core.Common.Command;

namespace Core.Command.Commands.EndAuction
{
    public class EndAuctionCommand : ICommand
    {
        public Guid AuctionId { get; }

        public EndAuctionCommand(Guid auctionId)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
        }
    }
}
