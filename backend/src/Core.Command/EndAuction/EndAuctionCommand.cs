using System;
using Core.Common.Interfaces;
using MediatR;

namespace Core.Command.EndAuction
{
    public class EndAuctionCommand : IRequest, ICommand
    {
        public Guid AuctionId { get; }

        public EndAuctionCommand(Guid auctionId)
        {
            AuctionId = auctionId;
        }
    }
}
