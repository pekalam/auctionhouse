using System;
using Core.Common;
using Core.Common.Command;
using Core.Common.Exceptions.Command;
using MediatR;

namespace Core.Command.EndAuction
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
