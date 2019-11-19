using System;
using Core.Common;
using Core.Common.Command;
using MediatR;

namespace Core.Command.EndAuction
{
    public class EndAuctionCommand : ICommand
    {
        public Guid AuctionId { get; }

        public EndAuctionCommand(Guid auctionId)
        {
            AuctionId = auctionId;
        }
    }
}
